using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurricane.ViewModelBase;
using System.IO;
using System.Windows;

namespace Hurricane.ViewModels
{
    class MainViewModel : PropertyChangedBase
    {
        #region Singleton & Constructor
        private static MainViewModel _instance;
        public static MainViewModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MainViewModel();
                return _instance;
            }
        }

        private MainViewModel()
        {

        }

        private Window BaseWindow;
        public Settings.HurricaneSettings MySettings { get; protected set; }
        private Utilities.KeyboardListener KListener;

        public void Loaded(Window window)
        {
            this.BaseWindow = window;
            MySettings = Settings.HurricaneSettings.Instance;

            MusicManager = new Music.MusicManager();
            MusicManager.CSCoreEngine.StartVisualization += CSCoreEngine_StartVisualization;
            MusicManager.CSCoreEngine.TrackChanged += CSCoreEngine_TrackChanged;
            MusicManager.CSCoreEngine.PositionChanged += CSCoreEngine_PositionChanged;
            MusicManager.LoadFromSettings();
            BaseWindow.LocationChanged += (s, e) => {
                if (EqualizerIsOpen) {
                    var rect = Utilities.WindowHelper.GetWindowRectangle(BaseWindow);
                    equalizerwindow.SetPosition(rect, BaseWindow.ActualWidth);
                };
            };
            KListener = new Utilities.KeyboardListener();
            KListener.KeyDown += KListener_KeyDown;
            Updater = new Settings.UpdateService(MySettings.Config.Language == "de" ? Settings.UpdateService.Language.German : Settings.UpdateService.Language.English);
            Updater.CheckForUpdates(BaseWindow);
        }
        #endregion

        #region Events
        public event EventHandler StartVisualization; //This is ok so, trust me ;)
        void CSCoreEngine_StartVisualization(object sender, EventArgs e)
        {
            if (StartVisualization != null) StartVisualization(sender, e);
        }

        public event EventHandler<Music.TrackChangedEventArgs> TrackChanged;
        void CSCoreEngine_TrackChanged(object sender, Music.TrackChangedEventArgs e)
        {
            if (TrackChanged != null) TrackChanged(sender, e);
        }

        public event EventHandler<Music.PositionChangedEventArgs> PositionChanged;
        void CSCoreEngine_PositionChanged(object sender, Music.PositionChangedEventArgs e)
        {
            if (PositionChanged != null) PositionChanged(sender, e);
        }

        void KListener_KeyDown(object sender, Utilities.RawKeyEventArgs args)
        {
            switch (args.Key)
            {
                case System.Windows.Input.Key.MediaPlayPause:
                    Application.Current.Dispatcher.Invoke(() => MusicManager.CSCoreEngine.TogglePlayPause());
                    break;
                case System.Windows.Input.Key.MediaPreviousTrack:
                    Application.Current.Dispatcher.Invoke(() => MusicManager.GoBackward());
                    break;
                case System.Windows.Input.Key.MediaNextTrack:
                    Application.Current.Dispatcher.Invoke(() => MusicManager.GoForward());
                    break;
            }
        }
        #endregion

        #region Methods
        void ImportFiles(string[] paths, Music.Playlist playlist, EventHandler finished = null)
        {
            Views.ProgressWindow progresswindow = new Views.ProgressWindow(Application.Current.FindResource("filesgetimported").ToString(), false) { Owner = BaseWindow };
            System.Threading.Thread t = new System.Threading.Thread(() =>
            {
                playlist.AddFiles((s, e) => { Application.Current.Dispatcher.Invoke(() => progresswindow.SetProgress(e.Percentage)); progresswindow.SetText(e.CurrentFile); progresswindow.SetTitle(string.Format(Application.Current.FindResource("filesgetimported").ToString(), e.FilesImported, e.TotalFiles)); }, true, paths); MusicManager.SaveToSettings(); MySettings.Save(); Application.Current.Dispatcher.Invoke(() => progresswindow.Close());
                if (finished != null) Application.Current.Dispatcher.Invoke(() => finished(this, EventArgs.Empty));
            });
            t.IsBackground = true;
            t.Start();
            progresswindow.ShowDialog();
        }

        public void DragDropFiles(string[] files)
        {
            List<string> paths = new List<string>();
            foreach (string file in files)
            {
                if (Music.Track.IsSupported(new FileInfo(file)))
                {
                    paths.Add(file);
                }
            }
            ImportFiles(paths.ToArray(), MusicManager.SelectedPlaylist);
        }

        public void Closing()
        {
            MusicManager.CSCoreEngine.StopPlayback();
            if (EqualizerIsOpen) equalizerwindow.Close();
            if (MusicManager != null)
            {
                MusicManager.SaveToSettings();
                MySettings.Save();
                MusicManager.Dispose();
            }
            if (KListener != null)
                KListener.Dispose();
        }

        private bool remember = false;
        private Music.Playlist rememberedplaylist;

        public void OpenFile(FileInfo file)
        {
            foreach (var playlist in MusicManager.Playlists)
            {
                foreach (var track in playlist.Tracks)
                {
                    if (track.Path == file.FullName)
                    {
                        MusicManager.PlayTrack(track, playlist);
                        return;
                    }
                }
            }

            Music.Playlist selectedplaylist = null;
            var config = Hurricane.Settings.HurricaneSettings.Instance.Config;

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
                if (remember && MusicManager.Playlists.Contains(rememberedplaylist))
                {
                    selectedplaylist = rememberedplaylist;
                }
                else
                {
                    Views.TrackImportWindow window = new Views.TrackImportWindow(musicmanager.Playlists, musicmanager.SelectedPlaylist, file.Name) { Owner = BaseWindow };
                    if (window.ShowDialog() == false) return;
                    selectedplaylist = window.SelectedPlaylist;
                    if (window.RememberChoice)
                    {
                        remember = true;
                        rememberedplaylist = window.SelectedPlaylist;
                        if (window.RememberAlsoAfterRestart)
                        {
                            config.RememberTrackImportPlaylist = true;
                            config.PlaylistToImportTrack = selectedplaylist.Name;
                        }
                    }
                }
            }

            ImportFiles(new string[] { file.FullName }, selectedplaylist, (s, e) => OpenFile(file));
        }

        public void MoveOut()
        {
            if (EqualizerIsOpen) { equalizerwindow.Close(); EqualizerIsOpen = false; }
        }

        #endregion

        #region Commands
        private RelayCommand openequalizer;
        private bool EqualizerIsOpen;
        Views.EqualizerWindow equalizerwindow;
        public RelayCommand OpenEqualizer
        {
            get
            {
                if (openequalizer == null)
                    openequalizer = new RelayCommand((object parameter) =>
                    {
                        if (!EqualizerIsOpen)
                        {
                            var rect = Utilities.WindowHelper.GetWindowRectangle(BaseWindow);
                            equalizerwindow = new Views.EqualizerWindow(MusicManager.CSCoreEngine, rect, BaseWindow.ActualWidth);
                            equalizerwindow.Closed += (s, e) => EqualizerIsOpen = false;
                            equalizerwindow.BeginCloseAnimation += (s, e) => BaseWindow.Activate();
                            equalizerwindow.Show();
                            EqualizerIsOpen = true;
                        }
                        else
                        {
                            equalizerwindow.Activate();
                        }
                    });
                return openequalizer;
            }
        }

        private RelayCommand reloadtrackinformations;
        public RelayCommand ReloadTrackInformations
        {
            get
            {
                if (reloadtrackinformations == null)
                    reloadtrackinformations = new RelayCommand((object parameter) => {

                        Views.ProgressWindow progresswindow = new Views.ProgressWindow(Application.Current.FindResource("loadtrackinformation").ToString(), false) { Owner = BaseWindow };
                        System.Threading.Thread t = new System.Threading.Thread(() =>
                        {
                            MusicManager.SelectedPlaylist.ReloadTrackInformations((s, e) => { Application.Current.Dispatcher.Invoke(() => progresswindow.SetProgress(e.Percentage)); progresswindow.SetText(e.CurrentFile); progresswindow.SetTitle(string.Format(Application.Current.FindResource("loadtrackinformation").ToString(), e.FilesImported, e.TotalFiles)); }, true); MusicManager.SaveToSettings(); MySettings.Save(); Application.Current.Dispatcher.Invoke(() => progresswindow.Close());
                        });
                        t.IsBackground = true;
                        t.Start();
                        progresswindow.ShowDialog();
                    });
                return reloadtrackinformations;
            }
        }

        private RelayCommand removemissingtracks;
        public RelayCommand RemoveMissingTracks
        {
            get
            {
                if (removemissingtracks == null)
                    removemissingtracks = new RelayCommand((object parameter) => {
                        Views.MessageWindow message = new Views.MessageWindow("suredeleteallmissingtracks", "removemissingtracks", true, true) { Owner = BaseWindow};
                        if (message.ShowDialog() == true)
                        {
                            MusicManager.SelectedPlaylist.RemoveMissingTracks();
                        }
                        MusicManager.SaveToSettings();
                        MySettings.Save();
                    });
                return removemissingtracks;
            }
        }

        private RelayCommand removeduplicatetracks;
        public RelayCommand RemoveDuplicateTracks
        {
            get
            {
                if (removeduplicatetracks == null)
                    removeduplicatetracks = new RelayCommand((object parameter) => {
                        Views.MessageWindow message = new Views.MessageWindow("removeduplicatetracksmessage", "removeduplicates", true, true);
                        message.Owner = this.BaseWindow;
                        if (message.ShowDialog() == true)
                        {
                            Views.ProgressWindow progresswindow = new Views.ProgressWindow(Application.Current.FindResource("removeduplicates").ToString(), true) { Owner = BaseWindow};
                            progresswindow.SetText(Application.Current.FindResource("searchingforduplicates").ToString());
                            
                            System.Threading.Thread t = new System.Threading.Thread(() => {
                                var counter = MusicManager.SelectedPlaylist.RemoveDuplicates(true);
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    progresswindow.Close();
                                    Views.MessageWindow successmessage = new Views.MessageWindow(counter == 0 ? Application.Current.FindResource("noduplicatesmessage").ToString() : string.Format(Application.Current.FindResource("tracksremoved").ToString(), counter), Application.Current.FindResource("removeduplicates").ToString(), false) { Owner = BaseWindow };
                                    successmessage.ShowDialog();
                                });
                            });
                            t.IsBackground = true;
                            t.Start();
                            progresswindow.Show();
                        }
                    });
                return removeduplicatetracks;
            }
        }

        private RelayCommand openqueuemanager;
        public RelayCommand OpenQueueManager
        {
            get
            {
                if (openqueuemanager == null)
                    openqueuemanager = new RelayCommand((object parameter) =>
                    {
                        Views.QueueManager window = new Views.QueueManager() { Owner = BaseWindow };
                        window.ShowDialog();
                    });
                return openqueuemanager;
            }
        }

        private RelayCommand addfilestoplaylist;
        public RelayCommand AddFilesToPlaylist
        {
            get
            {
                if (addfilestoplaylist == null)
                    addfilestoplaylist = new RelayCommand((object parameter) =>
                    {
                        Ookii.Dialogs.Wpf.VistaOpenFileDialog ofd = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
                        ofd.CheckFileExists = true;
                        ofd.Title = System.Windows.Application.Current.FindResource("selectfiles").ToString();
                        ofd.Filter = CSCore.Codecs.CodecFactory.SupportedFilesFilterEn;
                        ofd.Multiselect = true;
                        if (ofd.ShowDialog(BaseWindow) == true)
                        {
                            ImportFiles(ofd.FileNames, MusicManager.SelectedPlaylist);
                        }
                    });
                return addfilestoplaylist;
            }
        }

        private RelayCommand addfoldertoplaylist;
        public RelayCommand AddFolderToPlaylist
        {
            get
            {
                if (addfoldertoplaylist == null)
                    addfoldertoplaylist = new RelayCommand((object parameter) =>
                    {
                        Views.FolderImportWindow window = new Views.FolderImportWindow();
                        window.Owner = BaseWindow;
                        if (window.ShowDialog() == true)
                        {
                            DirectoryInfo di = new DirectoryInfo(window.SelectedPath);
                            List<string> filestoadd = new List<string>();
                            foreach (FileInfo fi in di.GetFiles("*.*", window.IncludeSubfolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                            {
                                if (Music.Track.IsSupported(fi))
                                {
                                    filestoadd.Add(fi.FullName);
                                }
                            }

                            ImportFiles(filestoadd.ToArray(), MusicManager.SelectedPlaylist);
                        }
                    });
                return addfoldertoplaylist;
            }
        }

        private RelayCommand addnewplaylist;
        public RelayCommand AddNewPlaylist
        {
            get
            {
                if (addnewplaylist == null)
                    addnewplaylist = new RelayCommand((object parameter) =>
                    {
                        Views.CreateNewPlaylistWindow window = new Views.CreateNewPlaylistWindow() { Owner = BaseWindow };
                        if (window.ShowDialog() == true)
                        {
                            Music.Playlist newplaylist = new Music.Playlist() { Name = window.PlaylistName };
                            MusicManager.Playlists.Add(newplaylist);
                            MusicManager.RegisterPlaylist(newplaylist);
                            MusicManager.SelectedPlaylist = newplaylist;
                            MusicManager.SaveToSettings();
                            MySettings.Save();
                        }
                    });
                return addnewplaylist;
            }
        }

        private RelayCommand removeselectedtrack;
        public RelayCommand RemoveSelectedTrack
        {
            get
            {
                if (removeselectedtrack == null)
                    removeselectedtrack = new RelayCommand((object parameter) =>
                    {
                        Music.Track track = MusicManager.SelectedTrack;
                        if (track == null) return;
                        if (track.IsPlaying)
                        {
                            Views.MessageWindow errorbox = new Views.MessageWindow(string.Format(Application.Current.FindResource("trackisplaying").ToString(), track.Title), Application.Current.FindResource("error").ToString(), false) { Owner = BaseWindow };
                            errorbox.ShowDialog();
                            return;
                        }
                        Views.MessageWindow messagebox = new Views.MessageWindow(string.Format(Application.Current.FindResource("removetracksmessage").ToString(), track.Title), Application.Current.FindResource("removetracks").ToString(), true) { Owner = BaseWindow };
                        if (messagebox.ShowDialog() == true)
                        {
                            MusicManager.SelectedPlaylist.TrackCollection.Remove(track);
                        }
                    });
                return removeselectedtrack;
            }
        }

        private RelayCommand opensettings;
        public RelayCommand OpenSettings
        {
            get
            {
                if (opensettings == null)
                    opensettings = new RelayCommand((object parameter) => { Views.SettingsWindow window = new Views.SettingsWindow() { Owner = BaseWindow }; window.ShowDialog(); });
                return opensettings;
            }
        }

        private RelayCommand removeplaylist;
        public RelayCommand RemovePlaylist
        {
            get
            {
                if (removeplaylist == null)
                    removeplaylist = new RelayCommand((object parameter) =>
                    {
                        if (MusicManager.Playlists.Count == 1)
                        {
                            Views.MessageWindow message = new Views.MessageWindow("errorcantdeleteplaylist", "error", false, true);
                            message.Owner = BaseWindow;
                            message.ShowDialog();
                            return;
                        }
                        Views.MessageWindow window = new Views.MessageWindow(string.Format(Application.Current.FindResource("reallydeleteplaylist").ToString(), MusicManager.SelectedPlaylist.Name), Application.Current.FindResource("removeplaylist").ToString(), true);
                        window.Owner = BaseWindow;
                        if (window.ShowDialog() == true)
                        {
                            Music.Playlist PlaylistToDelete = MusicManager.SelectedPlaylist;
                            Music.Playlist NewPlaylist = MusicManager.Playlists[0];
                            if (MusicManager.CurrentPlaylist == PlaylistToDelete)
                                MusicManager.CSCoreEngine.StopPlayback();
                            MusicManager.CurrentPlaylist = NewPlaylist;
                            MusicManager.Playlists.Remove(PlaylistToDelete);
                            MusicManager.SelectedPlaylist = NewPlaylist;
                        }
                    });
                return removeplaylist;
            }
        }

        private RelayCommand renameplaylist;
        public RelayCommand RenamePlaylist
        {
            get
            {
                if (renameplaylist == null)
                    renameplaylist = new RelayCommand((object parameter) =>
                    {
                        Views.CreateNewPlaylistWindow window = new Views.CreateNewPlaylistWindow(MusicManager.SelectedPlaylist.Name);
                        window.Owner = BaseWindow;
                        if (window.ShowDialog() == true) { MusicManager.SelectedPlaylist.Name = window.PlaylistName; }
                    });
                return renameplaylist;
            }
        }

        private RelayCommand opentrackinformations;
        public RelayCommand OpenTrackInformations
        {
            get
            {
                if (opentrackinformations == null)
                    opentrackinformations = new RelayCommand((object parameter) =>
                    {
                        Views.TrackInformationWindow window = new Views.TrackInformationWindow(MusicManager.SelectedTrack);
                        window.Owner = BaseWindow;
                        window.ShowDialog();
                    });
                return opentrackinformations;
            }
        }

        private RelayCommand openupdater;
        public RelayCommand OpenUpdater
        {
            get
            {
                if (openupdater == null)
                    openupdater = new RelayCommand((object parameter) =>
                    {
                        Views.UpdateWindow window = new Views.UpdateWindow(Updater) { Owner = BaseWindow };
                        window.ShowDialog();
                    });
                return openupdater;
            }
        }

        private RelayCommand clearselectedplaylist;
        public RelayCommand ClearSelectedPlaylist
        {
            get
            {
                if (clearselectedplaylist == null)
                    clearselectedplaylist = new RelayCommand((object parameter) =>
                    {
                        Views.MessageWindow window = new Views.MessageWindow(string.Format(Application.Current.FindResource("sureremovealltracks").ToString(), MusicManager.SelectedPlaylist.Name), Application.Current.FindResource("removealltracks").ToString(), true) { Owner = BaseWindow };
                        if (window.ShowDialog() == true)
                            MusicManager.SelectedPlaylist.TrackCollection.Clear();
                    });
                return clearselectedplaylist;
            }
        }
        #endregion

        #region Properties
        private Music.MusicManager musicmanager;
        public Music.MusicManager MusicManager
        {
            get { return musicmanager; }
            set
            {
                SetProperty(value, ref musicmanager);
            }
        }
        
        private Settings.UpdateService updater;
        public Settings.UpdateService Updater
        {
            get { return updater; }
            set
            {
                SetProperty(value, ref updater);
            }
        }
        #endregion
    }
}
