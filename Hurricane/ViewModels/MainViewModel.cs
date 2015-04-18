using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hurricane.DragDrop;
using Hurricane.Music;
using Hurricane.Music.CustomEventArgs;
using Hurricane.Music.Playlist;
using Hurricane.Music.Track;
using Hurricane.Settings;
using Hurricane.Utilities;
using Hurricane.ViewModelBase;
using Hurricane.Views;
using QueueManager = Hurricane.Views.QueueManagerWindow;

namespace Hurricane.ViewModels
{
    partial class MainViewModel : PropertyChangedBase
    {
        private static MainViewModel _instance;
        public static MainViewModel Instance
        {
            get { return _instance ?? (_instance = new MainViewModel()); }
        }

        private MainViewModel()
        {
        }

        private MainWindow _baseWindow;

        private HurricaneSettings _mySettings;
        public HurricaneSettings MySettings
        {
            get { return _mySettings; }
            protected set
            {
                SetProperty(value, ref _mySettings);
            }
        }

        private KeyboardListener _keyboardListener;

        public void Loaded(MainWindow window)
        {
            _baseWindow = window;
            MySettings = HurricaneSettings.Instance;

            MusicManager = new MusicManager();
            MusicManager.CSCoreEngine.TrackChanged += CSCoreEngine_TrackChanged;
            MusicManager.CSCoreEngine.ExceptionOccurred += CSCoreEngine_ExceptionOccurred;
            MusicManager.CSCoreEngine.SoundOutErrorOccurred += CSCoreEngine_SoundOutErrorOccurred;
            MusicManager.LoadFromSettings();
            MainTabControlIndex = MySettings.CurrentState.MainTabControlIndex;

            _keyboardListener = new KeyboardListener();
            _keyboardListener.KeyDown += KListener_KeyDown;
            Updater = new UpdateService(MySettings.Config.Language == "de" ? UpdateService.Language.German : UpdateService.Language.English);
            if (MySettings.Config.CheckForHurricaneUpdates) Updater.CheckForUpdates(_baseWindow);
        }

        #region Events

        public event EventHandler<TrackChangedEventArgs> TrackChanged;
        void CSCoreEngine_TrackChanged(object sender, TrackChangedEventArgs e)
        {
            if (TrackChanged != null) TrackChanged(sender, e);
        }

        async void CSCoreEngine_ExceptionOccurred(object sender, Exception e)
        {

            await _baseWindow.WindowDialogService.ShowMessage(Application.Current.Resources["ExceptionOpenOnlineTrack"].ToString(),
                Application.Current.Resources["Exception"].ToString(), false, DialogMode.Single);
        }

        async void CSCoreEngine_SoundOutErrorOccurred(object sender, string e)
        {
            await _baseWindow.WindowDialogService.ShowMessage(e, Application.Current.Resources["Exception"].ToString(), false, DialogMode.Single);
        }

        void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            switch (args.Key)
            {
                case Key.MediaPlayPause:
                    Application.Current.Dispatcher.Invoke(() => MusicManager.CSCoreEngine.TogglePlayPause());
                    break;
                case Key.MediaPreviousTrack:
                    Application.Current.Dispatcher.Invoke(() => MusicManager.GoBackward());
                    break;
                case Key.MediaNextTrack:
                    Application.Current.Dispatcher.Invoke(() => MusicManager.GoForward());
                    break;
            }
        }
        #endregion

        #region Methods

        async Task ImportFiles(IEnumerable<string> paths, NormalPlaylist playlist, EventHandler finished = null)
        {
            var controller = await _baseWindow.WindowDialogService.CreateProgressDialog(string.Empty, false);

            var tracks = Playlists.ImportFiles(paths, (s, e) =>
            {
                controller.SetProgress(e.Percentage);
                controller.SetMessage(e.CurrentFile);
                controller.SetTitle(string.Format(Application.Current.Resources["FilesGetImported"].ToString(), e.FilesImported, e.TotalFiles));
            });

            await playlist.AddFiles(tracks);

            MusicManager.SaveToSettings();
            MySettings.Save();
            await controller.Close();
            if (finished != null) Application.Current.Dispatcher.Invoke(() => finished(this, EventArgs.Empty));
        }

        // flatten out directories, if any, and return list of files
        IEnumerable<string> CollectFiles(IEnumerable<string> paths, Func<string, bool> isSupported)
        {
            var files = new List<string>();

            foreach (var path in paths)
            {
                var attribs = File.GetAttributes(path);

                if ((attribs & FileAttributes.Directory) == FileAttributes.Directory)
                    files.AddRange(Directory.GetFiles(path));
                else
                    files.Add(path);
            }

            return files.Where(isSupported);
        }

        // simple check using file extension
        bool IsFileSupported(string filePath)
        {
            return LocalTrack.IsSupported(new FileInfo(filePath)) || Playlists.IsSupported(filePath);
        }

        public async void DragDropFiles(string[] files)
        {
            if (!MusicManager.SelectedPlaylist.CanEdit) return;
            await ImportFiles(CollectFiles(files, IsFileSupported), (NormalPlaylist)MusicManager.SelectedPlaylist);
        }

        public void Closing()
        {
            if (MusicManager != null)
            {
                MusicManager.CSCoreEngine.StopPlayback();
                MusicManager.SaveToSettings();
                MySettings.CurrentState.MainTabControlIndex = MainTabControlIndex;
                MySettings.Save();
                MusicManager.Dispose();
            }
            if (_keyboardListener != null)
                _keyboardListener.Dispose();
            if (Updater != null) Updater.Dispose();
            HurricaneSettings.Instance.Config.AppCommunicationManager.Stop();
        }

        private bool _remember;
        private NormalPlaylist _rememberedPlaylist;

        public async void OpenFile(FileInfo file, bool play)
        {
            foreach (var playlist in MusicManager.Playlists)
            {
                foreach (var track in playlist.Tracks.Where(track => track.GetType() == typeof(LocalTrack) && ((LocalTrack)track).Path == file.FullName))
                {
                    if (play) MusicManager.PlayTrack(track, playlist);
                    return;
                }
            }

            NormalPlaylist selectedplaylist = null;
            var config = HurricaneSettings.Instance.Config;

            if (config.RememberTrackImportPlaylist)
            {
                var items = MusicManager.Playlists.Where(x => x.Name == config.PlaylistToImportTrack).ToList();
                if (items.Any())
                {
                    selectedplaylist = items.First();
                }
                else { config.RememberTrackImportPlaylist = false; config.PlaylistToImportTrack = null; }
            }

            if (selectedplaylist == null)
            {
                if (_remember && MusicManager.Playlists.Contains(_rememberedPlaylist))
                {
                    selectedplaylist = _rememberedPlaylist;
                }
                else
                {
                    var selectedPlaylist = _musicmanager.SelectedPlaylist.CanEdit ? (NormalPlaylist)_musicmanager.SelectedPlaylist : _musicmanager.Playlists[0];
                    var window = new TrackImportWindow(_musicmanager.Playlists, selectedPlaylist, file.Name) { Owner = _baseWindow };
                    if (window.ShowDialog() == false) return;
                    selectedplaylist = window.SelectedPlaylist;
                    if (window.RememberChoice)
                    {
                        _remember = true;
                        _rememberedPlaylist = window.SelectedPlaylist;
                        if (window.RememberAlsoAfterRestart)
                        {
                            config.RememberTrackImportPlaylist = true;
                            config.PlaylistToImportTrack = selectedplaylist.Name;
                        }
                    }
                }
            }

            await ImportFiles(new[] { file.FullName }, selectedplaylist, (s, e) => OpenFile(file, play));
        }

        #endregion

        private MusicManager _musicmanager;
        public MusicManager MusicManager
        {
            get { return _musicmanager; }
            set
            {
                SetProperty(value, ref _musicmanager);
            }
        }

        private UpdateService _updater;
        public UpdateService Updater
        {
            get { return _updater; }
            set
            {
                SetProperty(value, ref _updater);
            }
        }

        private int _mainTabControlIndex;
        public int MainTabControlIndex
        {
            get { return _mainTabControlIndex; }
            set
            {
                SetProperty(value, ref _mainTabControlIndex);
            }
        }

        private TrackListDropHandler _trackListDropHandler;
        public TrackListDropHandler TrackListDropHandler
        {
            get { return _trackListDropHandler ?? (_trackListDropHandler = new TrackListDropHandler()); }
        }

        private PlaylistListDropHandler _playlistListDropHandler;
        public PlaylistListDropHandler PlaylistListDropHandler
        {
            get { return _playlistListDropHandler ?? (_playlistListDropHandler = new PlaylistListDropHandler()); }
        }
    }
}
