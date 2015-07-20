using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Hurricane.Model.DataApi;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.ViewModel.MainView.Base;

namespace Hurricane.ViewModel.MainView
{
    public class ChartsView : SideListItem, IPlaylist
    {
        private readonly static Random Random = new Random();

        private List<PreviewTrack> _chartList;
        private bool _isLoading;
        private RelayCommand _playChartTrackCommand;

        public override ViewCategorie ViewCategorie { get; } = ViewCategorie.Discover;
        public override Geometry Icon { get; } = (GeometryGroup)Application.Current.Resources["VectorCharts"];
        public override string Text => Application.Current.Resources["Charts"].ToString();

        public List<PreviewTrack> ChartList
        {
            get { return _chartList; }
            set { SetProperty(value, ref _chartList); }
        }

        public bool IsLoading //If the charts are loading
        {
            get { return _isLoading; }
            set { SetProperty(value, ref _isLoading); }
        }

        public RelayCommand PlayChartTrackCommand
        {
            get
            {
                return _playChartTrackCommand ?? (_playChartTrackCommand = new RelayCommand(async parameter =>
                {
                    var chartItem = parameter as PreviewTrack;
                    if (chartItem == null)
                        return;

                    await MusicDataManager.MusicManager.OpenPlayable(await GetPlayable(chartItem), this);
                    ViewController.SetIsPlaying(this);
                }));
            }
        }

        private async Task<IPlayable> GetPlayable(PreviewTrack track)
        {
            var result = await
                MusicDataManager.MusicStreamingPluginManager.DefaultMusicStreaming.MusicStreamingService
                    .GetTrack($"{track.Artist} - {track.Name}");
            result.Title = track.Name;
            result.Artist = track.Artist;
            result.Cover = track.Image;
            return result;
        }

        protected async override Task Load()
        {
            IsLoading = true;
            ChartList = await iTunesApi.GetTop100(CultureInfo.CurrentCulture);
            IsLoading = false;

            foreach (var previewTrack in ChartList)
                await previewTrack.Image.LoadImageAsync();
        }

        public Task<IPlayable> GetNextTrack(IPlayable currentTrack)
        {
            var newIndex =
                ChartList.IndexOf(ChartList.First(x => x.Name == currentTrack.Title && x.Artist == currentTrack.Artist)) +
                1;
            if (newIndex >= ChartList.Count)
                newIndex = 0;
            return GetPlayable(ChartList[newIndex]);
        }

        public Task<IPlayable> GetShuffleTrack()
        {
            var newIndex = Random.Next(0, ChartList.Count);
            return GetPlayable(ChartList[newIndex]);
        }

        public Task<IPlayable> GetPreviousTrack(IPlayable currentTrack)
        {
            var newIndex =
                ChartList.IndexOf(ChartList.First(x => x.Name == currentTrack.Title && x.Artist == currentTrack.Artist)) -
                1;
            if (newIndex < 0)
                newIndex = ChartList.Count - 1;

            return GetPlayable(ChartList[newIndex]);
        }

        public Task<IPlayable> GetLastTrack()
        {
            return GetPlayable(ChartList.Last());
        }

        public bool ContainsPlayableTracks()
        {
            return true;
        }
    }
}