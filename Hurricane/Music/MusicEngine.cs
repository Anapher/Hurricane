using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurricane.ViewModelBase;
using System.Collections.ObjectModel;

namespace Hurricane.Music
{
    class MusicEngine : PropertyChangedBase, IDisposable
    {
        #region GUIProperties
        private Track selectedtrack;
        public Track SelectedTrack
        {
            get { return selectedtrack; }
            set
            {
                SetProperty(value, ref selectedtrack);
            }
        }

        private bool repeattrack;
        public bool RepeatTrack
        {
            get { return repeattrack; }
            set
            {
                SetProperty(value, ref repeattrack);
            }
        }

        private bool randomtrack;
        public bool RandomTrack
        {
            get { return randomtrack; }
            set
            {
                SetProperty(value, ref randomtrack);
            }
        }

        private Playlist selectedplaylist;
        public Playlist SelectedPlaylist
        {
            get { return selectedplaylist; }
            set
            {
                SetProperty(value, ref selectedplaylist);
            }
        }
        
        private String searchtext;
        public String SearchText
        {
            get { return searchtext; }
            set
            {
                SetProperty(value, ref searchtext);
                if (SelectedPlaylist != null && SelectedPlaylist.ViewSource != null) SelectedPlaylist.ViewSource.Refresh();
            }
        }
        #endregion

        private ObservableCollection<Playlist> playlists;
        public ObservableCollection<Playlist> Playlists
        {
            get { return playlists; }
            set
            {
                SetProperty(value, ref playlists);
            }
        }

        //WARNING: The different between the Current- and the SelectedPlaylist is, that the current playlist is the playlist who is played. The selected playlist is the playlist the user sees (can be the same)
        private Playlist currentplaylist;
        public Playlist CurrentPlaylist
        {
            get { return currentplaylist; }
            set
            {
                SetProperty(value, ref currentplaylist);
            }
        }

        public CSCore CSCoreEngine { get; protected set; }
        public Notification.NotificationService Notification { get; set; }

        public MusicEngine()
        {
            CSCoreEngine = new CSCore();
            Playlists = new ObservableCollection<Playlist>();
            CSCoreEngine.TrackFinished += CSCoreEngine_TrackFinished;
            random = new Random();
            Notification = new Notification.NotificationService(CSCoreEngine);
        }

        void CSCoreEngine_TrackFinished(object sender, EventArgs e)
        {
            if (RepeatTrack)
            {
                CSCoreEngine.OpenFile(CSCoreEngine.CurrentTrack);
                CSCoreEngine.TogglePlayPause();
            }
            else
            {
                GoForward();
            }
        }

        public void LoadFromSettings()
        {
            Settings.HurricaneSettings settings = Settings.HurricaneSettings.Instance;
            this.Playlists = settings.Playlists.Playlists;
            Settings.ConfigSettings config = settings.Config;
            CSCoreEngine.EqualizerSettings = config.EqualizerSettings;
            CSCoreEngine.EqualizerSettings.Loaded();
            CSCoreEngine.Volume = config.Volume;
            if (config.LastPlaylistIndex > -1)
            {
                CurrentPlaylist = Playlists[config.LastPlaylistIndex];
            }

            if (config.LastTrackIndex > -1)
            {
                CSCoreEngine.OpenFile(CurrentPlaylist.Tracks[config.LastTrackIndex]);
                CSCoreEngine.Position = config.TrackPosition;
                CSCoreEngine.OnPropertyChanged("Position");
            }
            if (config.SelectedPlaylist > -1)
            {
                SelectedPlaylist = Playlists[config.SelectedPlaylist];
            }
            if (config.SelectedTrack > -1)
            {
                SelectedTrack = SelectedPlaylist.Tracks[config.SelectedTrack];
            }
            this.RepeatTrack = config.RepeatTrack;
            this.RandomTrack = config.RandomTrack;
            foreach (Playlist lst in Playlists)
            {
                lst.RefreshList(Predicate);
            }
        }

        protected bool Predicate(object item)
        {
            Track track = (Track)item;
            if (string.IsNullOrWhiteSpace(SearchText)) { return true; } else { return (track.Title.ToUpper().Contains(SearchText.ToUpper()) || (!string.IsNullOrEmpty(track.Artist) && track.Artist.ToUpper().StartsWith(SearchText.ToUpper()))); }
        }

        public void RegisterPlaylist(Playlist playlist)
        {
            playlist.RefreshList(Predicate);
        }

        public void SaveToSettings()
        {
            Settings.HurricaneSettings settings = Settings.HurricaneSettings.Instance;
            settings.Playlists.Playlists = this.Playlists;
            Settings.ConfigSettings config = settings.Config;
            config.Volume = CSCoreEngine.Volume;
            config.LastPlaylistIndex = CurrentPlaylist == null ? -1 : Playlists.IndexOf(CurrentPlaylist);
            config.LastTrackIndex = CSCoreEngine.CurrentTrack == null ? -1 : CurrentPlaylist.Tracks.IndexOf(CSCoreEngine.CurrentTrack);
            config.SelectedPlaylist = Playlists.IndexOf(SelectedPlaylist); //Its impossible that no playlist is selected
            config.SelectedTrack = SelectedTrack == null ? -1 : SelectedPlaylist.Tracks.IndexOf(SelectedTrack);
            config.RepeatTrack = this.RepeatTrack;
            config.RandomTrack = this.RandomTrack;
            config.TrackPosition = CSCoreEngine.CurrentTrack == null ? 0 : CSCoreEngine.Position;
            config.EqualizerSettings = CSCoreEngine.EqualizerSettings;
        }

        public void Dispose()
        {
            CSCoreEngine.Dispose();
        }

        #region Commands
        private RelayCommand playselectedtrack;
        public RelayCommand PlaySelectedTrack
        {
            get
            {
                if (playselectedtrack == null)
                    playselectedtrack = new RelayCommand((object parameter) =>
                    {
                        if (SelectedTrack != CSCoreEngine.CurrentTrack)
                        {
                            CSCoreEngine.StopPlayback();
                            CSCoreEngine.OpenFile(SelectedTrack);
                            CSCoreEngine.TogglePlayPause();
                            CurrentPlaylist = SelectedPlaylist;
                        }
                    });
                return playselectedtrack;
            }
        }

        private RelayCommand goforwardcommand;
        public RelayCommand GoForwardCommand
        {
            get
            {
                if (goforwardcommand == null)
                    goforwardcommand = new RelayCommand((object parameter) => { GoForward(); });
                return goforwardcommand;
            }
        }

        protected Random random;
        public void GoForward()
        {
            if (CurrentPlaylist == null || CurrentPlaylist.Tracks.Count == 0) return;
            CSCoreEngine.StopPlayback();
            int currenttrackindex = CurrentPlaylist.Tracks.IndexOf(CSCoreEngine.CurrentTrack);
            int nexttrackindex = 0;
            if (CurrentPlaylist.Tracks.Count > 1)
            {
                if (RandomTrack)
                {
                    while (true)
                    {
                        int i = random.Next(0, CurrentPlaylist.Tracks.Count);
                        if (i != currenttrackindex) { nexttrackindex = i; break; }
                    }
                }
                else
                {
                    nexttrackindex = currenttrackindex + 1;
                    if (CurrentPlaylist.Tracks.Count - 1 < nexttrackindex)
                    {
                        nexttrackindex = 0;
                    }
                }
            }
            CSCoreEngine.OpenFile(CurrentPlaylist.Tracks[nexttrackindex]);
            CSCoreEngine.TogglePlayPause();
        }

        private RelayCommand gobackwardcommand;
        public RelayCommand GoBackwardCommand
        {
            get
            {
                if (gobackwardcommand == null)
                    gobackwardcommand = new RelayCommand((object parameter) => { GoBackward(); });
                return gobackwardcommand;
            }
        }

        public void GoBackward()
        {
            if (CurrentPlaylist == null || CurrentPlaylist.Tracks.Count == 0) return;
            CSCoreEngine.StopPlayback();
            int currenttrackindex = CurrentPlaylist.Tracks.IndexOf(CSCoreEngine.CurrentTrack);
            int nexttrackindex = currenttrackindex - 1;
            if (0 > nexttrackindex)
            {
                nexttrackindex = CurrentPlaylist.Tracks.Count -1;
            }
            CSCoreEngine.OpenFile(CurrentPlaylist.Tracks[nexttrackindex]);
            CSCoreEngine.TogglePlayPause();
        }

        private RelayCommand openfilelocation;
        public RelayCommand OpenFileLocation
        {
            get
            {
                if (openfilelocation == null)
                    openfilelocation = new RelayCommand((object parameter) => { System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + SelectedTrack.Path + "\""); });
                return openfilelocation;
            }
        }
        #endregion
    }
}
