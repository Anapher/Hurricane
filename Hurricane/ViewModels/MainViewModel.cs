using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CSCore.Codecs;
using Hurricane.Music;
using Hurricane.Music.Data;
using Hurricane.Music.MusicDatabase.EventArgs;
using Hurricane.Music.Track;
using Hurricane.Settings;
using Hurricane.Utilities;
using Hurricane.ViewModelBase;
using Hurricane.Views;
using Microsoft.Win32;
using QueueManager = Hurricane.Views.QueueManagerWindow;

namespace Hurricane.ViewModels
{
    class MainViewModel : PropertyChangedBase
    {
        #region Singleton & Constructor
        private static MainViewModel _instance;
        public static MainViewModel Instance
        {
            get { return _instance ?? (_instance = new MainViewModel()); }
        }

        private MainViewModel()
        {
            TrackSearcher = new TrackSearcher();
        }

        private MainWindow _baseWindow;
        public HurricaneSettings MySettings { get; protected set; }
        private KeyboardListener _keyboardListener;

        public void Loaded(MainWindow window)
        {
            _baseWindow = window;
            MySettings = HurricaneSettings.Instance;

            MusicManager = new MusicManager();
            MusicManager.CSCoreEngine.StartVisualization += CSCoreEngine_StartVisualization;
            MusicManager.CSCoreEngine.TrackChanged += CSCoreEngine_TrackChanged;
            MusicManager.LoadFromSettings();

            _keyboardListener = new KeyboardListener();
            _keyboardListener.KeyDown += KListener_KeyDown;
            Updater = new UpdateService(MySettings.Config.Language == "de" ? UpdateService.Language.German : UpdateService.Language.English);
            Updater.CheckForUpdates(_baseWindow);
        }
        #endregion

        #region Events
        public event EventHandler StartVisualization; //This is ok so, trust me ;)
        void CSCoreEngine_StartVisualization(object sender, EventArgs e)
        {
            if (StartVisualization != null) StartVisualization(sender, e);
        }

        public event EventHandler<TrackChangedEventArgs> TrackChanged;
        void CSCoreEngine_TrackChanged(object sender, TrackChangedEventArgs e)
        {
            if (TrackChanged != null) TrackChanged(sender, e);
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
        async Task ImportFiles(string[] paths, Playlist playlist, EventHandler finished = null)
        {
            var controller = _baseWindow.Messages.CreateProgressDialog(string.Empty, false);

            await playlist.AddFiles((s, e) =>
            {
                controller.SetProgress(e.Percentage);
                controller.SetMessage(e.CurrentFile);
                controller.SetTitle(string.Format(Application.Current.Resources["FilesGetImported"].ToString(), e.FilesImported, e.TotalFiles));
            }, paths);

            MusicManager.SaveToSettings();
            MySettings.Save();
            await controller.Close();
            if (finished != null) Application.Current.Dispatcher.Invoke(() => finished(this, EventArgs.Empty));
        }

        public async void DragDropFiles(string[] files)
        {
            if (!MusicManager.SelectedPlaylist.CanEdit) return;
            await ImportFiles(files.Where(file => LocalTrack.IsSupported(new FileInfo(file))).ToArray(), (Playlist)MusicManager.SelectedPlaylist);
        }

        public void Closing()
        {
            MusicManager.CSCoreEngine.StopPlayback();
            if (MusicManager != null)
            {
                MusicManager.SaveToSettings();
                MySettings.Save();
                MusicManager.Dispose();
            }
            if (_keyboardListener != null)
                _keyboardListener.Dispose();
            if (Updater != null) Updater.Dispose();
        }

        private bool _remember = false;
        private Playlist _rememberedPlaylist;

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

            Playlist selectedplaylist = null;
            var config = HurricaneSettings.Instance.Config;

            if (config.RememberTrackImportPlaylist)
            {
                var items = MusicManager.Playlists.Where((x) => x.Name == config.PlaylistToImportTrack);
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
                    var _selectedPlaylist = _musicmanager.SelectedPlaylist.CanEdit ? (Playlist)_musicmanager.SelectedPlaylist : _musicmanager.Playlists[0];
                    TrackImportWindow window = new TrackImportWindow(_musicmanager.Playlists, _selectedPlaylist, file.Name) { Owner = _baseWindow };
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

        #region Commands



        private RelayCommand _openequalizer;
        public RelayCommand OpenEqualizer
        {
            get
            {
                return _openequalizer ?? (_openequalizer = new RelayCommand(async parameter =>
                {
                    await _baseWindow.ShowEqualizer();
                }));
            }
        }

        private RelayCommand _reloadtrackinformation;
        public RelayCommand ReloadTrackInformation
        {
            get
            {
                return _reloadtrackinformation ?? (_reloadtrackinformation = new RelayCommand(async parameter =>
                {
                    if (!MusicManager.SelectedPlaylist.CanEdit) return;
                    var controller = _baseWindow.Messages.CreateProgressDialog(string.Empty, false);

                    await ((Playlist)MusicManager.SelectedPlaylist).ReloadTrackInformation((s, e) =>
                    {
                        controller.SetProgress(e.Percentage);
                        controller.SetMessage(e.CurrentFile);
                        controller.SetTitle(string.Format(Application.Current.Resources["LoadTrackInformation"].ToString(), e.FilesImported, e.TotalFiles));
                    });

                    MusicManager.SaveToSettings();
                    MySettings.Save();
                    await controller.Close();
                }));
            }
        }

        private RelayCommand _removemissingtracks;
        public RelayCommand RemoveMissingTracks
        {
            get
            {
                return _removemissingtracks ?? (_removemissingtracks = new RelayCommand(async parameter =>
                {
                    if (MusicManager.SelectedPlaylist.CanEdit && await _baseWindow.ShowMessage(Application.Current.Resources["DeleteAllMissingTracks"].ToString(), Application.Current.Resources["RemoveMissingTracks"].ToString(), true, DialogMode.Single))
                    {
                        ((Playlist)MusicManager.SelectedPlaylist).RemoveMissingTracks();
                        MusicManager.SaveToSettings();
                        MySettings.Save();
                    }
                }));
            }
        }

        private RelayCommand _removeduplicatetracks;
        public RelayCommand RemoveDuplicateTracks
        {
            get
            {
                return _removeduplicatetracks ?? (_removeduplicatetracks = new RelayCommand(async parameter =>
                {
                    if (await _baseWindow.ShowMessage(Application.Current.Resources["RemoveDuplicateTracks"].ToString(), Application.Current.Resources["RemoveDuplicates"].ToString(), true, DialogMode.First))
                    {
                        var controller = _baseWindow.Messages.CreateProgressDialog(Application.Current.Resources["RemoveDuplicates"].ToString(), true);
                        controller.SetMessage(Application.Current.Resources["SearchingForDuplicates"].ToString());

                        var counter = await ((PlaylistBase)MusicManager.SelectedPlaylist).RemoveDuplicates();
                        await controller.Close();
                        await _baseWindow.ShowMessage(counter == 0 ? Application.Current.Resources["NoDuplicatesFound"].ToString() : string.Format(Application.Current.Resources["TracksRemoved"].ToString(), counter), Application.Current.Resources["RemoveDuplicates"].ToString(), false, DialogMode.Last);
                    }
                }));
            }
        }

        private RelayCommand _openqueuemanager;
        public RelayCommand OpenQueueManager
        {
            get
            {
                return _openqueuemanager ?? (_openqueuemanager = new RelayCommand(parameter =>
                {
                    QueueManager window = new QueueManager() { Owner = _baseWindow };
                    window.ShowDialog();
                }));
            }
        }

        private RelayCommand _addfilestoplaylist;
        public RelayCommand AddFilesToPlaylist
        {
            get
            {
                return _addfilestoplaylist ?? (_addfilestoplaylist = new RelayCommand(async parameter =>
                {
                    if (!MusicManager.SelectedPlaylist.CanEdit) return;
                    var ofd = new OpenFileDialog
                    {
                        CheckFileExists = true,
                        Title = Application.Current.Resources["SelectedFiles"].ToString(),
                        Filter = CodecFactory.SupportedFilesFilterEn,
                        Multiselect = true
                    };
                    if (ofd.ShowDialog(_baseWindow) == true)
                        await ImportFiles(ofd.FileNames, (Playlist)MusicManager.SelectedPlaylist);
                }));
            }
        }

        private RelayCommand _addfoldertoplaylist;
        public RelayCommand AddFolderToPlaylist
        {
            get
            {
                return _addfoldertoplaylist ?? (_addfoldertoplaylist = new RelayCommand(async parameter =>
                {
                    if (!MusicManager.SelectedPlaylist.CanEdit) return;
                    FolderImportWindow window = new FolderImportWindow { Owner = _baseWindow };
                    if (window.ShowDialog() != true) return;
                    DirectoryInfo di = new DirectoryInfo(window.SelectedPath);
                    await ImportFiles((from fi in di.GetFiles("*.*", window.IncludeSubfolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly) where LocalTrack.IsSupported(fi) select fi.FullName).ToArray(), (Playlist)MusicManager.SelectedPlaylist);
                }));
            }
        }

        private RelayCommand _addnewplaylist;
        public RelayCommand AddNewPlaylist
        {
            get
            {
                return _addnewplaylist ?? (_addnewplaylist = new RelayCommand(async parameter =>
                {
                    string result = await _baseWindow.ShowInputDialog(Application.Current.Resources["NewPlaylist"].ToString(), Application.Current.Resources["NameOfPlaylist"].ToString(), Application.Current.Resources["Create"].ToString(), string.Empty, DialogMode.Single);
                    if (string.IsNullOrEmpty(result)) return;
                    Playlist newplaylist = new Playlist() { Name = result };
                    MusicManager.Playlists.Add(newplaylist);
                    MusicManager.RegisterPlaylist(newplaylist);
                    MusicManager.SelectedPlaylist = newplaylist;
                    MusicManager.SaveToSettings();
                    MySettings.Save();
                }));
            }
        }

        private RelayCommand _removeselectedtracks;
        public RelayCommand RemoveSelectedTracks
        {
            get
            {
                return _removeselectedtracks ?? (_removeselectedtracks = new RelayCommand(async parameter =>
                {
                    var tracks = ((IList)parameter).Cast<PlayableBase>().ToList();
                    if (tracks.Count == 0) return;
                    if (await _baseWindow.ShowMessage(tracks.Count > 1 ? string.Format(Application.Current.Resources["RemoveTracksMessage"].ToString(), tracks.Count) : string.Format(Application.Current.Resources["RemoveTrackMessage"].ToString(), tracks[0].Title), Application.Current.Resources["RemoveTracks"].ToString(), true, DialogMode.Single))
                    {
                        foreach (var t in tracks)
                        {
                            if (t.IsPlaying)
                            {
                                MusicManager.CSCoreEngine.StopPlayback();
                                MusicManager.CSCoreEngine.KickTrack();
                            }
                            MusicManager.SelectedPlaylist.RemoveTrack(t);
                        }
                    }
                }));
            }
        }

        private RelayCommand _opensettings;
        public RelayCommand OpenSettings
        {
            get
            {
                return _opensettings ?? (_opensettings = new RelayCommand(parameter =>
                {
                    SettingsWindow window = new SettingsWindow() { Owner = _baseWindow };
                    window.ShowDialog();
                }));
            }
        }

        private RelayCommand _removeplaylist;
        public RelayCommand RemovePlaylist
        {
            get
            {
                return _removeplaylist ?? (_removeplaylist = new RelayCommand(async parameter =>
                {
                    if (!MusicManager.SelectedPlaylist.CanEdit) return;
                    if (MusicManager.Playlists.Count == 1)
                    {
                        await _baseWindow.ShowMessage(Application.Current.Resources["CantDeletePlaylist"].ToString(), Application.Current.Resources["Error"].ToString(), false, DialogMode.Single);
                        return;
                    }
                    if (await _baseWindow.ShowMessage(string.Format(Application.Current.Resources["ReallyDeletePlaylist"].ToString(), MusicManager.SelectedPlaylist.Name), Application.Current.Resources["RemovePlaylist"].ToString(), true, DialogMode.Single))
                    {
                        Playlist playlistToDelete = (Playlist)MusicManager.SelectedPlaylist;
                        Playlist newPlaylist = MusicManager.Playlists[0];
                        bool nexttrack = MusicManager.CurrentPlaylist == playlistToDelete;
                        MusicManager.CurrentPlaylist = newPlaylist;
                        if (nexttrack)
                        { MusicManager.CSCoreEngine.StopPlayback(); MusicManager.CSCoreEngine.KickTrack(); MusicManager.GoForward(); }
                        MusicManager.Playlists.Remove(playlistToDelete);
                        MusicManager.SelectedPlaylist = newPlaylist;
                    }
                }));
            }
        }

        private RelayCommand _renameplaylist;
        public RelayCommand RenamePlaylist
        {
            get
            {
                return _renameplaylist ?? (_renameplaylist = new RelayCommand(async parameter =>
                {
                    string result = await _baseWindow.ShowInputDialog(Application.Current.Resources["RenamePlaylist"].ToString(), Application.Current.Resources["NameOfPlaylist"].ToString(), Application.Current.Resources["Rename"].ToString(), MusicManager.SelectedPlaylist.Name, DialogMode.Single);
                    if (!string.IsNullOrEmpty(result)) { MusicManager.SelectedPlaylist.Name = result; }
                }));
            }
        }

        private RelayCommand _opentrackinformation;
        public RelayCommand OpenTrackInformation
        {
            get
            {
                return _opentrackinformation ?? (_opentrackinformation = new RelayCommand(parameter =>
                {
                    _baseWindow.OpenTrackInformations(MusicManager.SelectedTrack);
                }));
            }
        }

        private RelayCommand _opentageditor;
        public RelayCommand OpenTagEditor
        {
            get { return _opentageditor ?? (_opentageditor = new RelayCommand(parameter =>
            {
                var localtrack = MusicManager.SelectedTrack as LocalTrack;
                if (localtrack == null) return;
                _baseWindow.OpenTagEditor(localtrack);
            })); }
        }

        private RelayCommand _openupdater;
        public RelayCommand OpenUpdater
        {
            get
            {
                return _openupdater ?? (_openupdater = new RelayCommand(parameter =>
                {
                    UpdateWindow window = new UpdateWindow(Updater) { Owner = _baseWindow };
                    window.ShowDialog();
                }));
            }
        }

        private RelayCommand _clearselectedplaylist;
        public RelayCommand ClearSelectedPlaylist
        {
            get
            {
                return _clearselectedplaylist ?? (_clearselectedplaylist = new RelayCommand(async parameter =>
                {
                    if (await _baseWindow.ShowMessage(string.Format(Application.Current.Resources["RemoveAllTracksQuestion"].ToString(), MusicManager.SelectedPlaylist.Name), Application.Current.Resources["RemoveAllTracks"].ToString(), true, DialogMode.Single))
                    {
                        MusicManager.SelectedPlaylist.Clear();
                    }
                }));
            }
        }

        private RelayCommand _toggleVolume;
        private float oldVolume;
        public RelayCommand ToggleVolume
        {
            get { return _toggleVolume ?? (_toggleVolume = new RelayCommand(parameter =>
            {
                if (MusicManager.CSCoreEngine.Volume == 0)
                {
                    MusicManager.CSCoreEngine.Volume = oldVolume;
                }
                else
                {
                    oldVolume = MusicManager.CSCoreEngine.Volume;
                    MusicManager.CSCoreEngine.Volume = 0;
                }
            })); }
        }

        private RelayCommand _openOnlineSection;
        public RelayCommand OpenOnlineSection
        {
            get { return _openOnlineSection ?? (_openOnlineSection = new RelayCommand(parameter =>
            {
                MainTabControlIndex = 3;
            })); }
        }
        #endregion

        #region Properties
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

        public TrackSearcher TrackSearcher { get; set; }

        private int _mainTabControlIndex;
        public int MainTabControlIndex
        {
            get { return _mainTabControlIndex; }
            set
            {
                SetProperty(value, ref _mainTabControlIndex);
            }
        }

        #endregion

    }
}
