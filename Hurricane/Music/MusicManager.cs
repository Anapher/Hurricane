using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Hurricane.Music.API;
using Hurricane.Music.Data;
using Hurricane.Music.Download;
using Hurricane.Music.MusicDatabase.EventArgs;
using Hurricane.Music.Playlist;
using Hurricane.Music.Track;
using Hurricane.Notification;
using Hurricane.Settings;
using Hurricane.ViewModelBase;

namespace Hurricane.Music
{
    class MusicManager : PropertyChangedBase, IDisposable
    {
        #region Public Properties
        private PlayableBase _selectedtrack;
        public PlayableBase SelectedTrack
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

        private IPlaylist _selectedplaylist;
        public IPlaylist SelectedPlaylist
        {
            get { return _selectedplaylist; }
            set
            {
                if (SetProperty(value, ref _selectedplaylist))
                    OnPropertyChanged("FavoriteListIsSelected");
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

        private ObservableCollection<NormalPlaylist> _playlists;
        public ObservableCollection<NormalPlaylist> Playlists
        {
            get { return _playlists; }
            set
            {
                SetProperty(value, ref _playlists);
            }
        }

        //WARNING: The different between the Current- and the SelectedPlaylist is, that the current playlist is the playlist who is played. The selected playlist is the playlist the user sees (can be the same)
        private IPlaylist _currentplaylist;
        public IPlaylist CurrentPlaylist
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

        public DownloadManager DownloadManager { get; set; }

        protected FavoriteList favoritePlaylist;
        public FavoriteList FavoritePlaylist
        {
            get { return favoritePlaylist; }
        }

        public bool FavoriteListIsSelected {
            get { return SelectedPlaylist == FavoritePlaylist; }
            set
            {
                if (value)
                {
                    SelectedPlaylist = null;
                    SelectedPlaylist = FavoritePlaylist;
                }
                else
                {
                    SelectedPlaylist = Playlists[0];
                }

            }
        }

        #endregion

        #region Contructor and Loading
        public MusicManager()
        {
            CSCoreEngine = new CSCoreEngine();
            Playlists = new ObservableCollection<NormalPlaylist>();
            CSCoreEngine.TrackFinished += CSCoreEngine_TrackFinished;
            CSCoreEngine.TrackChanged += CSCoreEngine_TrackChanged;
            Notification = new NotificationService(CSCoreEngine);
            Commands = new MusicManagerCommands(this);

            Random = new Random();
            Lasttracks = new List<TrackPlaylistPair>();
            Queue = new QueueManager();

            ApiServer = new TcpServer(this);
            if (HurricaneSettings.Instance.Config.ApiIsEnabled) ApiServer.StartListening();
            DownloadManager = new DownloadManager();
        }

        public async void LoadFromSettings()
        {
            HurricaneSettings settings = HurricaneSettings.Instance;
            Playlists = settings.Playlists.Playlists;
            ConfigSettings config = settings.Config;
            CSCoreEngine.EqualizerSettings = config.EqualizerSettings;
            CSCoreEngine.EqualizerSettings.Loaded();
            CSCoreEngine.Volume = config.Volume;

            favoritePlaylist = new FavoriteList();
            favoritePlaylist.Initalize(this.Playlists);

            if (config.LastPlaylistIndex > -10)
            {
                CurrentPlaylist = IndexToPlaylist(config.LastPlaylistIndex);
            }

            SelectedPlaylist = IndexToPlaylist(config.SelectedPlaylist);

            if (config.SelectedTrack > -1)
            {
                SelectedTrack = SelectedPlaylist.Tracks[config.SelectedTrack];
            }
            IsLoopEnabled = config.IsLoopEnabled;
            IsShuffleEnabled = config.IsShuffleEnabled;
            foreach (NormalPlaylist lst in Playlists)
            {
                lst.LoadList();
            }
            favoritePlaylist.LoadList();
            if (config.Queue != null) { Queue = config.Queue; Queue.Initialize(Playlists); }

            if (config.LastTrackIndex > -1)
            {
                PlayableBase t = CurrentPlaylist.Tracks[config.LastTrackIndex];
                if (t.TrackExists)
                {
                    await CSCoreEngine.OpenTrack(t);
                    CSCoreEngine.Position = config.TrackPosition;
                    CSCoreEngine.OnPropertyChanged("Position");
                }
            }

            AsyncTrackLoader.Instance.RunAsync(Playlists.ToList());
           
        }
        #endregion

        #region Event Handler
        async void CSCoreEngine_TrackFinished(object sender, EventArgs e)
        {
            if (IsLoopEnabled)
            {
                await CSCoreEngine.OpenTrack(CSCoreEngine.CurrentTrack);
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
        public void RegisterPlaylist(NormalPlaylist playlist)
        {
            playlist.LoadList();
        }

        public async void PlayTrack(PlayableBase track, IPlaylist playlist)
        {
            CSCoreEngine.StopPlayback();
            await CSCoreEngine.OpenTrack(track);
            CSCoreEngine.TogglePlayPause();
            CurrentPlaylist = playlist;
        }

        public async void GoForward()
        {
            if (CurrentPlaylist == null || CurrentPlaylist.Tracks.Count == 0) return;
            PlayableBase nexttrack;

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
                        var nextTrack = CurrentPlaylist.GetRandomTrack(CSCoreEngine.CurrentTrack);
                        if (nextTrack == null) return;
                        nexttrackindex = CurrentPlaylist.Tracks.IndexOf(nextTrack);
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

            await CSCoreEngine.OpenTrack(nexttrack);
            CSCoreEngine.TogglePlayPause();
        }

        private bool CheckIfTracksExists(IPlaylist list)
        {
            int counter = 0;
            bool result = false;
            foreach (PlayableBase t in list.Tracks)
            {
                t.RefreshTrackExists();
                if (t.TrackExists) { counter++; if (counter == 2) result = true; } //Don't cancel because all tracks need to refresh
            }
            return result;
        }

        private bool _openedTrackWithStandardBackward;
        public async void GoBackward()
        {
            if (CurrentPlaylist == null || CurrentPlaylist.Tracks.Count == 0) return;
            PlayableBase newtrack;
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
            await CSCoreEngine.OpenTrack(newtrack);
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
            config.LastPlaylistIndex = CurrentPlaylist == null ? -1 : PlaylistToIndex(CurrentPlaylist);
            config.LastTrackIndex = CSCoreEngine.CurrentTrack == null ? -1 : CurrentPlaylist.Tracks.IndexOf(CSCoreEngine.CurrentTrack);
            config.SelectedPlaylist = PlaylistToIndex(SelectedPlaylist); //Its impossible that no playlist is selected
            config.SelectedTrack = SelectedTrack == null ? -1 : SelectedPlaylist.Tracks.IndexOf(SelectedTrack);
            config.IsLoopEnabled = IsLoopEnabled;
            config.IsShuffleEnabled = IsShuffleEnabled;
            config.TrackPosition = CSCoreEngine.CurrentTrack == null ? 0 : CSCoreEngine.Position;
            config.EqualizerSettings = CSCoreEngine.EqualizerSettings;
            config.Queue = Queue.Count > 0 ? Queue : null;
        }

        public IPlaylist IndexToPlaylist(int index)
        {
            switch (index)
            {
                case -1:
                    return FavoritePlaylist;
                case -10:
                    return Playlists[0];
                default:
                    return Playlists[index];
            }
        }

        public int PlaylistToIndex(IPlaylist playlist)
        {
            if (playlist is NormalPlaylist) return Playlists.IndexOf((NormalPlaylist)playlist);
            if (playlist is FavoriteList) return -1;
            return -10;
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
                DownloadManager.Dispose();
            }
        }

        #endregion
    }
}