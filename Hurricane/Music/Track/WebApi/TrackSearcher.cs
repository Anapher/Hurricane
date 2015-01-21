using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Hurricane.Music.Playlist;
using Hurricane.Music.Track.WebApi.SoundCloudApi;
using Hurricane.Music.Track.WebApi.YouTubeApi;
using Hurricane.Settings;
using Hurricane.Utilities;
using Hurricane.ViewModelBase;

namespace Hurricane.Music.Track.WebApi
{
    class TrackSearcher : PropertyChangedBase
    {
        public string SearchText { get; set; }
        public ObservableCollection<WebTrackResultBase> Results { get; set; }

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

        
        private IPlaylistResult _playlistResult;
        public IPlaylistResult PlaylistResult
        {
            get { return _playlistResult; }
            set
            {
                SetProperty(value, ref _playlistResult);
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
                    PlaylistResult = null;
                    try
                    {
                        var specialYouTubeResult = await YouTubeApi.YouTubeApi.CheckForSpecialUrl(SearchText);
                        if (specialYouTubeResult.Item1)
                        {
                            SortResults(specialYouTubeResult.Item2);
                            PlaylistResult = specialYouTubeResult.Item3;
                        }
                        else
                        {
                            Task<List<SoundCloudWebTrackResult>> tSoundCloud = null;
                            Task<List<YouTubeWebTrackResult>> tYouTube = null;
                            var service = _manager.DownloadManager.SelectedService;

                            if (service == 0 || service == 2)
                                tSoundCloud = SoundCloudApi.SoundCloudApi.Search(SearchText);

                            if (service == 0 || service == 1)
                                tYouTube = YouTubeApi.YouTubeApi.Search(SearchText);

                            var list = new List<WebTrackResultBase>();
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
                            _manager.DownloadManager.Searches.Add(SearchText);
                        }
                    }
                    catch (WebException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    
                    
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
                    if (!(await SelectedTrack.CheckIfAvailable()))
                    {
                        await _baseWindow.ShowMessage(Application.Current.Resources["ExceptionAddingOnlineTracks"].ToString(), Application.Current.Resources["Exception"].ToString(), false, DialogMode.Single);
                        return;
                    }
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

        private RelayCommand _addPlaylistToNewPlaylist;
        public RelayCommand AddPlaylistToNewPlaylist
        {
            get
            {
                return _addPlaylistToNewPlaylist ?? (_addPlaylistToNewPlaylist = new RelayCommand(async parameter =>
                {
                    if (PlaylistResult == null) return;
                    string result = await _baseWindow.ShowInputDialog(Application.Current.Resources["NewPlaylist"].ToString(), Application.Current.Resources["NameOfPlaylist"].ToString(), Application.Current.Resources["Create"].ToString(), PlaylistResult.Title, DialogMode.Single);
                    if (string.IsNullOrEmpty(result)) return;
                    NormalPlaylist playlist = new NormalPlaylist() { Name = result };
                    _manager.Playlists.Add(playlist);
                    _manager.RegisterPlaylist(playlist);

                    await AddTracksToPlaylist(playlist, PlaylistResult);
                    ViewModels.MainViewModel.Instance.MainTabControlIndex = 0;
                    _manager.SelectedPlaylist = playlist;
                }));
            }
        }

        private RelayCommand _addPlaylistToExisitingPlaylist;
        public RelayCommand AddPlaylistToExisitingPlaylist
        {
            get
            {
                return _addPlaylistToExisitingPlaylist ?? (_addPlaylistToExisitingPlaylist = new RelayCommand(async parameter =>
                {
                    if (PlaylistResult == null) return;
                    var playlist = parameter as NormalPlaylist;
                    if (playlist == null) return;
                    await AddTracksToPlaylist(playlist, PlaylistResult);
                    ViewModels.MainViewModel.Instance.MainTabControlIndex = 0;
                    _manager.SelectedPlaylist = playlist;
                }));
            }
        }

        private async Task AddTracksToPlaylist(NormalPlaylist playlist, IPlaylistResult result)
        {
            await Task.Delay(1000);
            var controller = _baseWindow.Messages.CreateProgressDialog(string.Format(Application.Current.Resources["AddTracksToPlaylist"].ToString(), playlist.Name), false);

            PlaylistResult.LoadingTracksProcessChanged += (s, e) =>
            {
                controller.SetMessage(string.Format(Application.Current.Resources["LoadingTracks"].ToString(), e.CurrentTrackName, e.Value, e.Maximum));
                controller.SetProgress(e.Value / e.Maximum);
            };
            foreach (var track in await PlaylistResult.GetTracks())
            {
                playlist.AddTrack(track);
            }
            _manager.SaveToSettings();
            HurricaneSettings.Instance.Save();
            await controller.Close();
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

        private readonly MusicManager _manager;
        private readonly MainWindow _baseWindow;
        public TrackSearcher(MusicManager manager, MainWindow baseWindow)
        {
            Results = new ObservableCollection<WebTrackResultBase>();
            cancelWaiter = new AutoResetEvent(false);
            _manager = manager;
            this._baseWindow = baseWindow;
        }
    }
}
