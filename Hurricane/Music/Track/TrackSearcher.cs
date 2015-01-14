using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hurricane.Music.Track.SoundCloudApi;
using Hurricane.Music.Track.YouTubeApi;
using Hurricane.Utilities;
using Hurricane.ViewModelBase;

namespace Hurricane.Music.Track
{
    public class TrackSearcher
    {
        public string SearchText { get; set; }
        public ObservableCollection<WebTrackResultBase> Results { get; set; }

        public bool LoadFromSoundCloud { get; set; }
        public bool LoadFromYouTube { get; set; }

        private readonly AutoResetEvent cancelWaiter;
        private bool _IsRunning;
        private bool _canceled;

        private RelayCommand _searchCommand;
        public RelayCommand SearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new RelayCommand(async parameter =>
                {
                    if (_IsRunning)
                    {
                        _canceled = true;
                        await Task.Run(() => cancelWaiter.WaitOne());
                    }

                    _IsRunning = true;
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

                    SortResults(list);

                    foreach (var track in Results)
                    {
                        await track.DownloadImage();
                        if (CheckForCanceled()) return;
                    }
                    _IsRunning = false;
                }));
            }
        }

        private bool CheckForCanceled()
        {
            Debug.Print(_canceled.ToString());
            if (_canceled)
            {
                cancelWaiter.Set();
                _canceled = false;
                _IsRunning = false;
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

        public TrackSearcher()
        {
            Results = new ObservableCollection<WebTrackResultBase>();
            LoadFromSoundCloud = true;
            LoadFromYouTube = true;
            cancelWaiter = new AutoResetEvent(false);
        }
    }
}
