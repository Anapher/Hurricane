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

            MusicEngine = new Music.MusicEngine();
            MusicEngine.CSCoreEngine.StartVisualization += CSCoreEngine_StartVisualization;
            MusicEngine.CSCoreEngine.TrackChanged += CSCoreEngine_TrackChanged;
            MusicEngine.LoadFromSettings();
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
                    Application.Current.Dispatcher.Invoke(() => MusicEngine.CSCoreEngine.TogglePlayPause());
                    break;
                case System.Windows.Input.Key.MediaPreviousTrack:
                    Application.Current.Dispatcher.Invoke(() => MusicEngine.GoBackward());
                    break;
                case System.Windows.Input.Key.MediaNextTrack:
                    Application.Current.Dispatcher.Invoke(() => MusicEngine.GoForward());
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
                        Ookii.Dialogs.Wpf.VistaFolderBrowserDialog fbd = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                        fbd.RootFolder = Environment.SpecialFolder.MyMusic;
                        fbd.ShowNewFolderButton = false;
                        fbd.Description = System.Windows.Application.Current.FindResource("selectfolder").ToString();
                        fbd.UseDescriptionForTitle = true;
                        if (fbd.ShowDialog(BaseWindow) == true)
                        {
                            DirectoryInfo di = new DirectoryInfo(fbd.SelectedPath);
                            List<string> filestoadd = new List<string>();
                            foreach (FileInfo fi in di.GetFiles())
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
                            MusicEngine.Playlists.Add(newplaylist);
                            MusicEngine.RegisterPlaylist(newplaylist);
                            MusicEngine.SelectedPlaylist = newplaylist;
                            MusicEngine.SaveToSettings();
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
                        foreach (Music.Track t in MusicEngine.SelectedPlaylist.Tracks)
                        {
                            if (t.IsSelected == true && MusicEngine.SelectedPlaylist.Tracks.Contains(t))
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
                                MusicEngine.SelectedPlaylist.Tracks.Remove(t);
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
                    opensettings = new RelayCommand((object parameter) => { Views.SettingsWindow window = new Views.SettingsWindow() { Owner = BaseWindow }; SettingsViewModel.Instance.MusicEngine = this.MusicEngine; window.ShowDialog(); });
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
                        if (MusicEngine.Playlists.Count == 1)
                        {
                            Views.MessageWindow message = new Views.MessageWindow("errorcantdeleteplaylist", "error", false, true);
                            message.Owner = BaseWindow;
                            message.ShowDialog();
                            return;
                        }
                        Views.MessageWindow window = new Views.MessageWindow(string.Format(Application.Current.FindResource("reallydeleteplaylist").ToString(), MusicEngine.SelectedPlaylist.Name), Application.Current.FindResource("removeplaylist").ToString(), true);
                        window.Owner = BaseWindow;
                        if (window.ShowDialog() == true)
                        {
                            Music.Playlist PlaylistToDelete = MusicEngine.SelectedPlaylist;
                            Music.Playlist NewPlaylist = MusicEngine.Playlists[0];
                            if (MusicEngine.CurrentPlaylist == PlaylistToDelete)
                                MusicEngine.CSCoreEngine.StopPlayback();
                            MusicEngine.CurrentPlaylist = NewPlaylist;
                            MusicEngine.Playlists.Remove(PlaylistToDelete);
                            MusicEngine.SelectedPlaylist = NewPlaylist;
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
                        Views.CreateNewPlaylistWindow window = new Views.CreateNewPlaylistWindow(MusicEngine.SelectedPlaylist.Name);
                        window.Owner = BaseWindow;
                        if (window.ShowDialog() == true) { MusicEngine.SelectedPlaylist.Name = window.PlaylistName; }
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
                        Views.TrackInformationWindow window = new Views.TrackInformationWindow(MusicEngine.SelectedTrack);
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
            Views.ProgressWindow progresswindow = new Views.ProgressWindow(Application.Current.FindResource("filesgetimported").ToString()) { Owner = BaseWindow };
            System.Threading.Thread t = new System.Threading.Thread(() =>
            {
                MusicEngine.SelectedPlaylist.AddFiles((s, e) => { Application.Current.Dispatcher.Invoke(() => progresswindow.SetProgress(e.Percentage)); progresswindow.SetText(e.CurrentFile); progresswindow.SetTitle(string.Format(Application.Current.FindResource("filesgetimported").ToString(), e.FilesImported, e.TotalFiles)); }, true, paths); MusicEngine.SaveToSettings(); MySettings.Save(); Application.Current.Dispatcher.Invoke(() => progresswindow.Close());
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
            if (MusicEngine != null)
            {
                MusicEngine.SaveToSettings();
                MySettings.Save();
                MusicEngine.Dispose();
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
                            equalizerwindow = new Views.EqualizerWindow(MusicEngine.CSCoreEngine, rect.Left + BaseWindow.ActualWidth, rect.Top + 25);
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

        public void MoveOut()
        {
            if (EqualizerIsOpen) { equalizerwindow.Close(); EqualizerIsOpen = false; }
        }
        #endregion

        #region Properties
        private Music.MusicEngine musicengine;
        public Music.MusicEngine MusicEngine
        {
            get { return musicengine; }
            set
            {
                SetProperty(value, ref musicengine);
            }
        }
        #endregion
    }
}
