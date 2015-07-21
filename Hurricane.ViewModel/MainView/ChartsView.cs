using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Hurricane.Model;
using Hurricane.Model.DataApi;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.Model.Services;
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

                    ViewController.SetIsPlaying(this);
                    await MusicDataManager.MusicManager.OpenPlayable(await GetPlayable(chartItem), this);
                }));
            }
        }

        private async Task<IPlayable> GetPlayable(PreviewTrack track)
        {
            var name = Regex.Match(track.Name, @"^(?<name>(.[^\(\[]+))").Groups["name"].Value;
            var result = await MusicDataManager.SearchTrack(track.Artist, name);
            result.Tag = track;

            var searchResult = result as ISearchResult;
            if (searchResult != null)
            {
                searchResult.Title = track.Name;
                searchResult.Artist = track.Artist;
                searchResult.Cover = track.Image;
            }

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
            return GetPlayable(ChartList.GetNextObject(currentTrack.Tag));
        }

        public Task<IPlayable> GetShuffleTrack()
        {
            return GetPlayable(ChartList.GetRandomObject());
        }

        public Task<IPlayable> GetPreviousTrack(IPlayable currentTrack)
        {
            return GetPlayable(ChartList.GetPreviousObject(currentTrack.Tag));
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