using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CSCore.Codecs;
using Hurricane.DragDrop;
using Hurricane.Music;
using Hurricane.Music.Download;
using Hurricane.Music.MusicDatabase.EventArgs;
using Hurricane.Music.Playlist;
using Hurricane.Music.Track;
using Hurricane.Music.Track.WebApi;
using Hurricane.Settings;
using Hurricane.Utilities;
using Hurricane.ViewModelBase;
using Hurricane.Views;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using WPFFolderBrowser;
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
            MusicManager.LoadFromSettings();
            TrackSearcher = new TrackSearcher(MusicManager, window);

            _keyboardListener = new KeyboardListener();
            _keyboardListener.KeyDown += KListener_KeyDown;
            Updater = new UpdateService(MySettings.Config.Language == "de" ? UpdateService.Language.German : UpdateService.Language.English);
            Updater.CheckForUpdates(_baseWindow);
        }
        #endregion

        #region Events

        public event EventHandler<TrackChangedEventArgs> TrackChanged;
        void CSCoreEngine_TrackChanged(object sender, TrackChangedEventArgs e)
        {
            if (TrackChanged != null) TrackChanged(sender, e);
        }

        async void CSCoreEngine_ExceptionOccurred(object sender, Exception e)
        {
            await _baseWindow.ShowMessage(Application.Current.Resources["ExceptionOpenOnlineTrack"].ToString(), Application.Current.Resources["Exception"].ToString(), false, DialogMode.Single);
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
        async Task ImportFiles(string[] paths, NormalPlaylist playlist, EventHandler finished = null)
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
            await ImportFiles(files.Where(file => LocalTrack.IsSupported(new FileInfo(file))).ToArray(), (NormalPlaylist)MusicManager.SelectedPlaylist);
        }

        public void Closing()
        {
            if (MusicManager != null)
            {
                MusicManager.CSCoreEngine.StopPlayback();
                MusicManager.SaveToSettings();
                MySettings.Save();
                MusicManager.Dispose();
            }
            if (_keyboardListener != null)
                _keyboardListener.Dispose();
            if (Updater != null) Updater.Dispose();
        }

        private bool _remember = false;
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
                    var _selectedPlaylist = _musicmanager.SelectedPlaylist.CanEdit ? (NormalPlaylist)_musicmanager.SelectedPlaylist : _musicmanager.Playlists[0];
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
        private RelayCommand _toggleEqualizer;
        public RelayCommand ToggleEqualizer
        {
            get
            {
                return _toggleEqualizer ?? (_toggleEqualizer = new RelayCommand(parameter =>
                {
                    _baseWindow.ShowEqualizer();
                }));
            }
        }

        private RelayCommand _closeEqualizer;
        public RelayCommand CloseEqualizer
        {
            get
            {
                return _closeEqualizer ?? (_closeEqualizer = new RelayCommand(parameter =>
                {
                    HurricaneSettings.Instance.CurrentState.EqualizerIsOpen = false;
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

                    await ((NormalPlaylist)MusicManager.SelectedPlaylist).ReloadTrackInformation((s, e) =>
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
                        ((NormalPlaylist)MusicManager.SelectedPlaylist).RemoveMissingTracks();
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
                        await ImportFiles(ofd.FileNames, (NormalPlaylist)MusicManager.SelectedPlaylist);
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
                    await ImportFiles((from fi in di.GetFiles("*.*", window.IncludeSubfolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly) where LocalTrack.IsSupported(fi) select fi.FullName).ToArray(), (NormalPlaylist)MusicManager.SelectedPlaylist);
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
                    NormalPlaylist newplaylist = new NormalPlaylist() { Name = result };
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
                    if (parameter == null) return;
                    var tracks = ((IList)parameter).Cast<PlayableBase>().ToList();
                    if (tracks.Count == 0) return;
                    if (await _baseWindow.ShowMessage(tracks.Count > 1 ? string.Format(Application.Current.Resources["RemoveTracksMessage"].ToString(), tracks.Count) : string.Format(Application.Current.Resources["RemoveTrackMessage"].ToString(), tracks[0].Title), Application.Current.Resources["RemoveTracks"].ToString(), true, DialogMode.Single))
                    {
                        foreach (var t in tracks)
                        {
                            if (t.IsOpened)
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
                        NormalPlaylist playlistToDelete = (NormalPlaylist)MusicManager.SelectedPlaylist;
                        NormalPlaylist newPlaylist = MusicManager.Playlists.First(x => x != playlistToDelete);
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
            get
            {
                return _opentageditor ?? (_opentageditor = new RelayCommand(parameter =>
                {
                    var localtrack = MusicManager.SelectedTrack as LocalTrack;
                    if (localtrack == null) return;
                    _baseWindow.OpenTagEditor(localtrack);
                }));
            }
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
            get
            {
                return _toggleVolume ?? (_toggleVolume = new RelayCommand(parameter =>
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
                }));
            }
        }

        private RelayCommand _openOnlineSection;
        public RelayCommand OpenOnlineSection
        {
            get
            {
                return _openOnlineSection ?? (_openOnlineSection = new RelayCommand(parameter =>
                {
                    MainTabControlIndex = 3;
                }));
            }
        }

        private RelayCommand _openDownloadManager;
        public RelayCommand OpenDownloadManager
        {
            get
            {
                return _openDownloadManager ?? (_openDownloadManager = new RelayCommand(parameter =>
                {
                    MusicManager.DownloadManager.IsOpen = true;
                }));
            }
        }

        private RelayCommand _convertStreamToLocalTrack;
        public RelayCommand ConvertStreamToLocalTrack
        {
            get
            {
                return _convertStreamToLocalTrack ?? (_convertStreamToLocalTrack = new RelayCommand(async parameter =>
                {
                    var track = MusicManager.SelectedTrack as StreamableBase;
                    if (track == null) return;
                    if (!track.CanDownload) return;

                    var sfd = new SaveFileDialog
                    {
                        Filter =
                            string.Format("M4A {0}|*.m4a|{1}|*.*", Application.Current.Resources["File"],
                                Application.Current.Resources["AllFiles"]),
                        FileName = track.Title
                    };
                    if (sfd.ShowDialog() == true)
                    {
                        var downloadFile = new FileInfo(sfd.FileName);
                        if (downloadFile.Exists) downloadFile.Delete();

                        var controller = await _baseWindow.ShowProgressAsync(Application.Current.Resources["Download"].ToString(), MusicManager.SelectedTrack.Title);
                        if (await
                            DownloadManager.DownloadTrack(track, sfd.FileName,
                                d =>
                                {
                                    controller.SetProgress(d / 100);
                                }))
                        {
                            if (MySettings.Config.Downloader.AddTagsToDownloads)
                                await DownloadManager.AddTags(track, sfd.FileName);
                            var newTrack = new LocalTrack { Path = sfd.FileName };
                            if (await newTrack.LoadInformation())
                            {
                                newTrack.TimeAdded = track.TimeAdded;
                                newTrack.Artist = track.Artist;
                                newTrack.Year = track.Year;
                                newTrack.Title = track.Title;
                                newTrack.Album = track.Album;
                                newTrack.Genres = track.Genres;
                                MusicManager.SelectedPlaylist.Tracks[MusicManager.SelectedPlaylist.Tracks.IndexOf(track)] = newTrack;
                                if (track.IsOpened)
                                    MusicManager.PlayTrack(newTrack, MusicManager.SelectedPlaylist);

                                await controller.CloseAsync();
                            }
                            else
                            {
                                await controller.CloseAsync();
                                await
                                    _baseWindow.ShowMessage(
                                        Application.Current.Resources["ExceptionConvertTrack"].ToString(),
                                        Application.Current.Resources["Exception"].ToString(), false, DialogMode.Single);
                            }
                        }
                        else
                        {
                            await controller.CloseAsync();
                            await _baseWindow.ShowMessage(Application.Current.Resources["ExceptionConvertTrack"].ToString(), Application.Current.Resources["Exception"].ToString(), false, DialogMode.Single);
                        }
                    }
                }));
            }
        }

        private RelayCommand _downloadAllStreams;
        public RelayCommand DownloadAllStreams
        {
            get
            {
                return _downloadAllStreams ?? (_downloadAllStreams = new RelayCommand(async parameter =>
                {
                    if (MusicManager.FavoriteListIsSelected) return;
                    var lst = MusicManager.SelectedPlaylist.Tracks.OfType<StreamableBase>().Where(x => x.CanDownload).ToList();
                    if (!lst.Any()) return;
                    var fdb = new WPFFolderBrowserDialog
                    {
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                        Title = MusicManager.SelectedPlaylist.Name
                    };

                    if (fdb.ShowDialog() != true) return;
                    var controller = await _baseWindow.ShowProgressAsync(Application.Current.Resources["Download"].ToString(), "", true);
                    foreach (var track in lst)
                    {
                        if (controller.IsCanceled)
                        {
                            await controller.CloseAsync();
                            return;
                        }
                        controller.SetMessage(track.Title);
                        var downloadFile = new FileInfo(Path.Combine(fdb.FileName, track.DownloadFilename));
                        if (downloadFile.Exists) continue;
                        if (await
                            DownloadManager.DownloadTrack(track, downloadFile.FullName,
                                d =>
                                {
                                    controller.SetProgress(lst.IndexOf(track) / (double)lst.Count + 1 / (double)lst.Count / 100 * d);
                                }))
                        {
                            if (MySettings.Config.Downloader.AddTagsToDownloads)
                                await DownloadManager.AddTags(track, downloadFile.FullName);
                            var newTrack = new LocalTrack { Path = downloadFile.FullName };
                            if (await newTrack.LoadInformation())
                            {
                                newTrack.TimeAdded = track.TimeAdded;
                                newTrack.Artist = track.Artist;
                                newTrack.Year = track.Year;
                                newTrack.Title = track.Title;
                                newTrack.Album = track.Album;
                                newTrack.Genres = track.Genres;
                                MusicManager.SelectedPlaylist.Tracks[MusicManager.SelectedPlaylist.Tracks.IndexOf(track)] = newTrack;
                            }
                        }
                    }
                    MusicManager.SaveToSettings();
                    HurricaneSettings.Instance.Save();
                    await controller.CloseAsync();
                }));
            }
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

        private TrackSearcher _trackSearcher;
        public TrackSearcher TrackSearcher
        {
            get { return _trackSearcher; }
            set
            {
                SetProperty(value, ref _trackSearcher);
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

        #endregion

        private TrackListDropHandler trackListDropHandler;
        public TrackListDropHandler TrackListDropHandler
        {
            get { return trackListDropHandler ?? (trackListDropHandler = new TrackListDropHandler()); }
        }

        private PlaylistListDropHandler playlistListDropHandler;
        public PlaylistListDropHandler PlaylistListDropHandler
        {
            get { return playlistListDropHandler ?? (playlistListDropHandler = new PlaylistListDropHandler()); }
        }
    }
}
