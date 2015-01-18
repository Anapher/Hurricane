using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hurricane.Music.Playlist;
using Hurricane.Music.Track.SoundCloudApi;
using Hurricane.Music.Track.YouTubeApi;
using Hurricane.Utilities;
using Hurricane.ViewModelBase;

namespace Hurricane.Music.Track
{
    class TrackSearcher : PropertyChangedBase
    {
        public string SearchText { get; set; }
        public ObservableCollection<WebTrackResultBase> Results { get; set; }

        public bool LoadFromSoundCloud { get; set; }
        public bool LoadFromYouTube { get; set; }

        private readonly AutoResetEvent cancelWaiter;
        private bool _IsSearching; //Difference between _IsRunning and IsSearching: _IsRunning is also true if pictures are downloading
        private bool _canceled;

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                SetProperty(value, ref _isLoading);
            }
        }

        private bool _nothingFound;
        public bool NothingFound
        {
            get { return _nothingFound; }
            set
            {
                SetProperty(value, ref _nothingFound);
            }
        }

        private WebTrackResultBase _selectedTrack;
        public WebTrackResultBase SelectedTrack
        {
            get { return _selectedTrack; }
            set
            {
                SetProperty(value, ref _selectedTrack);
            }
        }
        
        private RelayCommand _searchCommand;
        public RelayCommand SearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new RelayCommand(async parameter =>
                {
                    IsLoading = true;
                    if (_IsSearching)
                    {
                        _canceled = true;
                        await Task.Run(() => cancelWaiter.WaitOne());
                    }

                    _IsSearching = true;
                    Task<List<SoundCloudWebTrackResult>> tSoundCloud = null;
                    Task<List<YouTubeWebTrackResult>> tYouTube = null;

                    if (LoadFromSoundCloud)
                        tSoundCloud = SoundCloudApi.SoundCloudApi.Search(SearchText);

                    if (LoadFromYouTube)
                        tYouTube = YouTubeApi.YouTubeApi.Search(SearchText);

                    List<WebTrackResultBase> list = new List<WebTrackResultBase>();
                    if (tSoundCloud != null)
                    {
                        var result = await tSoundCloud;
                        result.ForEach(x => list.Add(x));
                    }
                    if (CheckForCanceled()) return;
                    if (tYouTube != null)
                    {
                        var result = await tYouTube;
                        result.ForEach(x => list.Add(x));
                    }

                    NothingFound = list.Count == 0;
                    SortResults(list);
                    
                    IsLoading = false;

                    foreach (var track in Results)
                    {
                        await track.DownloadImage();
                        if (CheckForCanceled()) return;
                    }
                    _IsSearching = false;
                }));
            }
        }

        private RelayCommand _playSelectedTrack;
        public RelayCommand PlaySelectedTrack
        {
            get { return _playSelectedTrack ?? (_playSelectedTrack = new RelayCommand(async parameter =>
            {
                if (SelectedTrack == null) return;
                IsLoading = true;
                await _manager.CSCoreEngine.OpenTrack(await SelectedTrack.ToPlayable());
                IsLoading = false;
                _manager.CSCoreEngine.TogglePlayPause();
            })); }
        }

        private RelayCommand _addToPlaylist;
        public RelayCommand AddToPlaylist
        {
            get
            {
                return _addToPlaylist ?? (_addToPlaylist = new RelayCommand(async parameter =>
                {
                    var playlist = parameter as IPlaylist;
                    if (playlist == null) return;
                    IsLoading = true;
                    var track = await SelectedTrack.ToPlayable();
                    track.TimeAdded = DateTime.Now;
                    playlist.AddTrack(track);
                    IsLoading = false;
                    ViewModels.MainViewModel.Instance.MainTabControlIndex = 0;
                    _manager.SelectedPlaylist = playlist;
                    _manager.SelectedTrack = track;
                }));
            }
        }
            
        private RelayCommand _downloadTrack;
        public RelayCommand DownloadTrack
        {
            get
            {
                return _downloadTrack ?? (_downloadTrack = new RelayCommand(parameter =>
                {
                    _manager.DownloadManager.AddEntry(SelectedTrack);
                    _manager.DownloadManager.IsOpen = true;
                }));
            }
        }

        private bool CheckForCanceled()
        {
            if (_canceled)
            {
                cancelWaiter.Set();
                _canceled = false;
                _IsSearching = false;
                return true;
            }
            return false;
        }

        private void SortResults(IEnumerable<WebTrackResultBase> list)
        {
            Results.Clear();
            foreach (var track in list.OrderBy(x => LevenshteinDistance.Compute(SearchText, x.Title)).ToList())
            {
                Results.Add(track);
            }
        }

        private MusicManager _manager;
        public TrackSearcher(MusicManager manager)
        {
            Results = new ObservableCollection<WebTrackResultBase>();
            LoadFromSoundCloud = true;
            LoadFromYouTube = true;
            cancelWaiter = new AutoResetEvent(false);
            _manager = manager;
        }
    }
}
