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
            MySettings.Load();

            MusicManager = new Music.MusicManager();
            MusicManager.CSCoreEngine.StartVisualization += CSCoreEngine_StartVisualization;
            MusicManager.CSCoreEngine.TrackChanged += CSCoreEngine_TrackChanged;
            MusicManager.LoadFromSettings();
            BaseWindow.LocationChanged += (s, e) => {
                if (EqualizerIsOpen) {
                    var rect = Utilities.WindowHelper.GetWindowRectangle(BaseWindow);
                    equalizerwindow.SetLeft(rect.Left + BaseWindow.ActualWidth); equalizerwindow.Top = rect.Top + 25;
                };
            };
            KListener = new Utilities.KeyboardListener();
            KListener.KeyDown += KListener_KeyDown;
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

        #region Commands
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
                            ImportFiles(ofd.FileNames);
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

                            ImportFiles(filestoadd.ToArray());
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

        private RelayCommand removeselectedtracks;
        public RelayCommand RemoveSelectedTracks
        {
            get
            {
                if (removeselectedtracks == null)
                    removeselectedtracks = new RelayCommand((object parameter) => {
                        List<Music.Track> tracks = new List<Music.Track>();
                        foreach (Music.Track t in MusicManager.SelectedPlaylist.Tracks)
                        {
                            if (t.IsSelected == true && MusicManager.SelectedPlaylist.Tracks.Contains(t))
                            {
                                if (t.IsPlaying)
                                {
                                    Views.MessageWindow errorbox = new Views.MessageWindow(string.Format(Application.Current.FindResource("trackisplaying").ToString(), t.Title), Application.Current.FindResource("error").ToString(), false) { Owner = BaseWindow };
                                    errorbox.ShowDialog();
                                    return;
                                }
                                tracks.Add(t);
                            }
                        }
                        Views.MessageWindow messagebox = new Views.MessageWindow(string.Format(Application.Current.FindResource("removetracksmessage").ToString(), tracks.Count), Application.Current.FindResource("removetracks").ToString(), true) { Owner = BaseWindow };
                        if (messagebox.ShowDialog() == true)
                        {
                            foreach (Music.Track t in tracks)
                            {
                                MusicManager.SelectedPlaylist.Tracks.Remove(t);
                            }
                        }
                    });
                return removeselectedtracks;
            }
        }

        private RelayCommand opensettings;
        public RelayCommand OpenSettings
        {
            get
            {
                if (opensettings == null)
                    opensettings = new RelayCommand((object parameter) => { Views.SettingsWindow window = new Views.SettingsWindow() { Owner = BaseWindow }; SettingsViewModel.Instance.MusicManager = this.MusicManager; window.ShowDialog(); });
                return opensettings;
            }
        }

        private RelayCommand removeplaylist;
        public RelayCommand RemovePlaylist
        {
            get
            {
                if (removeplaylist == null)
                    removeplaylist = new RelayCommand((object parameter) => {
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
                    renameplaylist = new RelayCommand((object parameter) => {
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
                    opentrackinformations = new RelayCommand((object parameter) => {
                        Views.TrackInformationWindow window = new Views.TrackInformationWindow(MusicManager.SelectedTrack);
                        window.Owner = BaseWindow;
                        window.ShowDialog();
                    });
                return opentrackinformations;
            }
        }
        #endregion

        #region Methods

        void ImportFiles(string[] paths)
        {
            Views.ProgressWindow progresswindow = new Views.ProgressWindow(Application.Current.FindResource("filesgetimported").ToString(), false) { Owner = BaseWindow };
            System.Threading.Thread t = new System.Threading.Thread(() =>
            {
                MusicManager.SelectedPlaylist.AddFiles((s, e) => { Application.Current.Dispatcher.Invoke(() => progresswindow.SetProgress(e.Percentage)); progresswindow.SetText(e.CurrentFile); progresswindow.SetTitle(string.Format(Application.Current.FindResource("filesgetimported").ToString(), e.FilesImported, e.TotalFiles)); }, true, paths); MusicManager.SaveToSettings(); MySettings.Save(); Application.Current.Dispatcher.Invoke(() => progresswindow.Close());
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
            ImportFiles(paths.ToArray());
        }

        public void Closing()
        {
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
        #endregion

        #region equalizer

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
                            equalizerwindow = new Views.EqualizerWindow(MusicManager.CSCoreEngine, rect.Left + BaseWindow.ActualWidth, rect.Top + 25);
                            equalizerwindow.Closed += (s, e) => EqualizerIsOpen = false;
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
                                var counter = MusicManager.SelectedPlaylist.RemoveDuplicates();
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

        public void MoveOut()
        {
            if (EqualizerIsOpen) { equalizerwindow.Close(); EqualizerIsOpen = false; }
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
        #endregion
    }
}
