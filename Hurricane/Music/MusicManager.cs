using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Hurricane.Music.API;
using Hurricane.Music.MusicDatabase.EventArgs;
using Hurricane.Notification;
using Hurricane.Settings;
using Hurricane.ViewModelBase;

namespace Hurricane.Music
{
    class MusicManager : PropertyChangedBase, IDisposable
    {
        #region Public Properties
        private Track _selectedtrack;
        public Track SelectedTrack
        {
            get { return _selectedtrack; }
            set
            {
                SetProperty(value, ref _selectedtrack);
            }
        }

        private bool _isloopenabled;
        public bool IsLoopEnabled
        {
            get { return _isloopenabled; }
            set
            {
                if (SetProperty(value, ref _isloopenabled) && value) IsShuffleEnabled = false;
            }
        }

        private bool _isshuffleenabled;
        public bool IsShuffleEnabled
        {
            get { return _isshuffleenabled; }
            set
            {
                if (SetProperty(value, ref _isshuffleenabled) && value) IsLoopEnabled = false;
            }
        }

        private Playlist _selectedplaylist;
        public Playlist SelectedPlaylist
        {
            get { return _selectedplaylist; }
            set
            {
                SetProperty(value, ref _selectedplaylist);
            }
        }

        private String _searchtext;
        public String SearchText
        {
            get { return _searchtext; }
            set
            {
                SetProperty(value, ref _searchtext);
                if (SelectedPlaylist != null && SelectedPlaylist.ViewSource != null) SelectedPlaylist.ViewSource.Refresh();
            }
        }

        private ObservableCollection<Playlist> _playlists;
        public ObservableCollection<Playlist> Playlists
        {
            get { return _playlists; }
            set
            {
                SetProperty(value, ref _playlists);
            }
        }

        //WARNING: The different between the Current- and the SelectedPlaylist is, that the current playlist is the playlist who is played. The selected playlist is the playlist the user sees (can be the same)
        private Playlist _currentplaylist;
        public Playlist CurrentPlaylist
        {
            get { return _currentplaylist; }
            set
            {
                SetProperty(value, ref _currentplaylist);
            }
        }

        public CSCoreEngine CSCoreEngine { get; protected set; }

        public NotificationService Notification { get; set; }

        public MusicManagerCommands Commands { get; protected set; }

        public QueueManager Queue { get; set; }

        public TcpServer ApiServer { get; set; }
        #endregion

        #region Contructor and Loading
        public MusicManager()
        {
            CSCoreEngine = new CSCoreEngine();
            Playlists = new ObservableCollection<Playlist>();
            CSCoreEngine.TrackFinished += CSCoreEngine_TrackFinished;
            CSCoreEngine.TrackChanged += CSCoreEngine_TrackChanged;
            Notification = new NotificationService(CSCoreEngine);
            Commands = new MusicManagerCommands(this);

            Random = new Random();
            Lasttracks = new List<TrackPlaylistPair>();
            Queue = new QueueManager();

            ApiServer = new TcpServer(this);
            if (HurricaneSettings.Instance.Config.ApiIsEnabled) ApiServer.StartListening();
        }

        public void LoadFromSettings()
        {
            HurricaneSettings settings = HurricaneSettings.Instance;
            Playlists = settings.Playlists.Playlists;
            ConfigSettings config = settings.Config;
            CSCoreEngine.EqualizerSettings = config.EqualizerSettings;
            CSCoreEngine.EqualizerSettings.Loaded();
            CSCoreEngine.Volume = config.Volume;
            if (config.LastPlaylistIndex > -1)
            {
                CurrentPlaylist = Playlists[config.LastPlaylistIndex];
            }

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

            if (config.SelectedPlaylist > -1)
            {
                SelectedPlaylist = Playlists[config.SelectedPlaylist];
            }
            if (config.SelectedTrack > -1)
            {
                SelectedTrack = SelectedPlaylist.Tracks[config.SelectedTrack];
            }
            IsLoopEnabled = config.IsLoopEnabled;
            IsShuffleEnabled = config.IsShuffleEnabled;
            foreach (Playlist lst in Playlists)
            {
                lst.LoadList();
            }
            if (config.Queue != null) { Queue = config.Queue; Queue.Initialize(Playlists.ToList()); }
        }
        #endregion

        #region Event Handler
        void CSCoreEngine_TrackFinished(object sender, EventArgs e)
        {
            if (IsLoopEnabled)
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
            if (_openedTrackWithStandardBackward) { _openedTrackWithStandardBackward = false; return; }
            if (Lasttracks.Count == 0 || Lasttracks.Last().Track != e.NewTrack)
                Lasttracks.Add(new TrackPlaylistPair(e.NewTrack, CurrentPlaylist));
        }
        #endregion

        #region Protected Members and Methods
        protected Random Random;
        protected List<TrackPlaylistPair> Lasttracks;

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

            if (Queue.HasTracks)
            {
                var tuple = Queue.PlayNextTrack();
                nexttrack = tuple.Item1;
                CurrentPlaylist = tuple.Item2;
            }
            else
            {
                int currenttrackindex = CurrentPlaylist.Tracks.IndexOf(CSCoreEngine.CurrentTrack);
                int nexttrackindex = currenttrackindex;
                if (CheckIfTracksExists(CurrentPlaylist))
                {
                    if (IsShuffleEnabled)
                    {
                        if (CurrentPlaylist.ShuffleList.Count == 0) CurrentPlaylist.ShuffleList = new List<Track>(CurrentPlaylist.Tracks);
                        bool hasrefreshed = false;
                        while (true)
                        {
                            int i = Random.Next(0, CurrentPlaylist.ShuffleList.Count);
                            if (i != currenttrackindex && CurrentPlaylist.ShuffleList[i].TrackExists)
                            {
                                nexttrackindex = CurrentPlaylist.Tracks.IndexOf(CurrentPlaylist.ShuffleList[i]);
                                CurrentPlaylist.ShuffleList.RemoveAt(i);
                                break;
                            }
                            else
                            {
                                CurrentPlaylist.ShuffleList.RemoveAt(i);
                                if (CurrentPlaylist.ShuffleList.Count == 0)
                                {
                                    if (hasrefreshed) continue;
                                    CurrentPlaylist.ShuffleList = new List<Track>(CurrentPlaylist.Tracks); hasrefreshed = true;
                                }
                            }
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

        private bool _openedTrackWithStandardBackward;
        public void GoBackward()
        {
            if (CurrentPlaylist == null || CurrentPlaylist.Tracks.Count == 0) return;
            Track newtrack;
            if (Lasttracks.Count > 1) //Check if there are more than two tracks, because the current track is the last one in the list
            {
                Lasttracks.Remove(Lasttracks.Last(x => x.Track == CSCoreEngine.CurrentTrack));
                newtrack = Lasttracks.Last().Track;
                CurrentPlaylist = Lasttracks.Last().Playlist;
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
                _openedTrackWithStandardBackward = true;
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
            HurricaneSettings settings = HurricaneSettings.Instance;
            settings.Playlists.Playlists = this.Playlists;
            ConfigSettings config = settings.Config;
            config.Volume = CSCoreEngine.Volume;
            config.LastPlaylistIndex = CurrentPlaylist == null ? -1 : Playlists.IndexOf(CurrentPlaylist);
            config.LastTrackIndex = CSCoreEngine.CurrentTrack == null ? -1 : CurrentPlaylist.Tracks.IndexOf(CSCoreEngine.CurrentTrack);
            config.SelectedPlaylist = Playlists.IndexOf(SelectedPlaylist); //Its impossible that no playlist is selected
            config.SelectedTrack = SelectedTrack == null ? -1 : SelectedPlaylist.Tracks.IndexOf(SelectedTrack);
            config.IsLoopEnabled = IsLoopEnabled;
            config.IsShuffleEnabled = IsShuffleEnabled;
            config.TrackPosition = CSCoreEngine.CurrentTrack == null ? 0 : CSCoreEngine.Position;
            config.EqualizerSettings = CSCoreEngine.EqualizerSettings;
            config.Queue = Queue.Count > 0 ? Queue : null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CSCoreEngine.Dispose();
                ApiServer.Dispose();
            }
        }

        #endregion
    }
}