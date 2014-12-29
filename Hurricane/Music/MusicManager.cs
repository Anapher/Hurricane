using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurricane.ViewModelBase;
using System.Collections.ObjectModel;

namespace Hurricane.Music
{
    class MusicManager : PropertyChangedBase, IDisposable
    {
        #region Public Properties
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
                if (SetProperty(value, ref repeattrack) && value) RandomTrack = false;
            }
        }

        private bool randomtrack;
        public bool RandomTrack
        {
            get { return randomtrack; }
            set
            {
                if (SetProperty(value, ref randomtrack) && value) RepeatTrack = false;
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

        public CSCoreEngine CSCoreEngine { get; protected set; }

        public Notification.NotificationService Notification { get; set; }

        public MusicManagerCommands Commands { get; protected set; }

        public QueueManager Queue { get; set; }

        public API.TcpServer ApiServer { get; set; }
        #endregion

        #region Contructor and Loading
        public MusicManager()
        {
            CSCoreEngine = new CSCoreEngine();
            Playlists = new ObservableCollection<Playlist>();
            CSCoreEngine.TrackFinished += CSCoreEngine_TrackFinished;
            CSCoreEngine.TrackChanged += CSCoreEngine_TrackChanged;
            Notification = new Notification.NotificationService(CSCoreEngine);
            this.Commands = new MusicManagerCommands(this);

            random = new Random();
            this.lasttracks = new List<TrackPlaylistPair>();
            this.Queue = new QueueManager();

            this.ApiServer = new API.TcpServer(this);
            if (Settings.HurricaneSettings.Instance.Config.ApiIsEnabled) ApiServer.StartListening();
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
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            if (config.LastTrackIndex > -1)
            {
                Track t = CurrentPlaylist.Tracks[config.LastTrackIndex];
                if (t.TrackExists)
                {
                    CSCoreEngine.OpenTrack(t);
                    CSCoreEngine.Position = config.TrackPosition;
                    CSCoreEngine.OnPropertyChanged("Position");
                }
            }
            System.Diagnostics.Debug.Print("MainViewModel: {0}", sw.ElapsedMilliseconds.ToString());
            sw.Stop();
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
                lst.LoadList();
            }
            if (config.Queue != null) { this.Queue = config.Queue; this.Queue.Initialize(Playlists.ToList()); }
        }
        #endregion

        #region Event Handler
        void CSCoreEngine_TrackFinished(object sender, EventArgs e)
        {
            if (RepeatTrack)
            {
                CSCoreEngine.OpenTrack(CSCoreEngine.CurrentTrack);
                CSCoreEngine.TogglePlayPause();
            }
            else
            {
                GoForward();
            }
        }

        void CSCoreEngine_TrackChanged(object sender, TrackChangedEventArgs e)
        {
            if (openedtrackwithstandardbackward) { openedtrackwithstandardbackward = false; return; }
            if (lasttracks.Count == 0 || !(lasttracks.Last().Track == e.NewTrack))
            {
                lasttracks.Add(new TrackPlaylistPair(e.NewTrack, this.CurrentPlaylist));
            }
        }
        #endregion

        #region Protected Members and Methods
        protected Random random;
        protected List<TrackPlaylistPair> lasttracks;

        #endregion

        #region Public Methods
        public void RegisterPlaylist(Playlist playlist)
        {
            playlist.LoadList();
        }

        public void PlayTrack(Track track, Playlist playlist)
        {
            CSCoreEngine.StopPlayback();
            CSCoreEngine.OpenTrack(track);
            CSCoreEngine.TogglePlayPause();
            CurrentPlaylist = playlist;
        }

        public void GoForward()
        {
            if (CurrentPlaylist == null || CurrentPlaylist.Tracks.Count == 0) return;
            Track nexttrack;

            if (this.Queue.HasTracks)
            {
                var tuple = Queue.PlayNextTrack();
                nexttrack = tuple.Item1;
                this.CurrentPlaylist = tuple.Item2;
            }
            else
            {
                int currenttrackindex = CurrentPlaylist.Tracks.IndexOf(CSCoreEngine.CurrentTrack);
                int nexttrackindex = currenttrackindex;
                if (CheckIfTracksExists(CurrentPlaylist))
                {
                    if (RandomTrack)
                    {
                        while (true)
                        {
                            int i = random.Next(0, CurrentPlaylist.Tracks.Count);
                            if (i != currenttrackindex && CurrentPlaylist.Tracks[i].TrackExists) { nexttrackindex = i; break; }
                        }
                    }
                    else
                    {
                        while (true)
                        {
                            nexttrackindex++;
                            if (CurrentPlaylist.Tracks.Count - 1 < nexttrackindex)
                                nexttrackindex = 0;
                            if (CurrentPlaylist.Tracks[nexttrackindex].TrackExists)
                                break;
                        }
                    }
                }
                nexttrack = CurrentPlaylist.Tracks[nexttrackindex];
            }

            CSCoreEngine.StopPlayback();
            CSCoreEngine.OpenTrack(nexttrack);
            CSCoreEngine.TogglePlayPause();
        }

        private bool CheckIfTracksExists(Playlist list)
        {
            int counter = 0;
            bool result = false;
            foreach (Track t in list.Tracks)
            {
                t.RefreshTrackExists();
                if (t.TrackExists) { counter++; if (counter == 2) result = true; } //Don't cancel because all tracks need to refresh
            }
            return result;
        }

        private bool openedtrackwithstandardbackward = false;
        public void GoBackward()
        {
            if (CurrentPlaylist == null || CurrentPlaylist.Tracks.Count == 0) return;
            Track newtrack;
            if (lasttracks.Count > 1) //Check if there are more than two tracks, because the current track is the last one in the list
            {
                lasttracks.Remove(lasttracks.Where((x) => x.Track == this.CSCoreEngine.CurrentTrack).Last());
                newtrack = lasttracks.Last().Track;
                this.CurrentPlaylist = lasttracks.Last().Playlist;
            }
            else
            {
                int currenttrackindex = CurrentPlaylist.Tracks.IndexOf(CSCoreEngine.CurrentTrack);
                int nexttrackindex = currenttrackindex;
                if (CheckIfTracksExists(CurrentPlaylist))
                {
                    while (true)
                    {
                        nexttrackindex--;
                        if (0 > nexttrackindex)
                        {
                            nexttrackindex = CurrentPlaylist.Tracks.Count - 1;
                        }
                        if (CurrentPlaylist.Tracks[nexttrackindex].TrackExists)
                            break;
                    }
                }
                openedtrackwithstandardbackward = true;
                newtrack = CurrentPlaylist.Tracks[nexttrackindex];
            }

            CSCoreEngine.StopPlayback();
            CSCoreEngine.OpenTrack(newtrack);
            CSCoreEngine.TogglePlayPause();
        }

        #endregion

        #region Save and Deconstruction
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
            config.Queue = this.Queue.Count > 0 ? this.Queue : null;
        }

        public void Dispose()
        {
            CSCoreEngine.Dispose();
            ApiServer.Dispose();
        }

        #endregion
    }
}
