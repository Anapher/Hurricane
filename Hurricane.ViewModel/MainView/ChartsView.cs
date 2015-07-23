using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hurricane.Model;
using Hurricane.Model.DataApi;
using Hurricane.Model.Music.Imagment;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.Model.Services;
using Hurricane.Utilities;
using Hurricane.ViewModel.MainView.Base;

namespace Hurricane.ViewModel.MainView
{
    public class ChartsView : SideListItem, IPlaylist
    {
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
                searchResult.Cover = new BitmapImageProvider(track.Image);
            }

            return result;
        }

        protected async override Task Load()
        {
            IsLoading = true;
            ChartList = await iTunesApi.GetTop100(CultureInfo.CurrentCulture);
            IsLoading = false;

            var imageFolder =
                new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Hurricane", "Images", "Charts"));
            if (!imageFolder.Exists)
                imageFolder.Create();

            var images = imageFolder.GetFiles("*.jpg").ToList();
            var usedImages = new List<FileInfo>();

            using (var webClient = new WebClient { Proxy = null })
            {
                foreach (var chart in ChartList)
                {
                    var imageFile = new FileInfo(Path.Combine(imageFolder.FullName, chart.ImageUrl.ToMd5Hash() + ".jpg"));
                    if (!imageFile.Exists)
                        await webClient.DownloadFileTaskAsync(chart.ImageUrl, imageFile.FullName);

                    chart.Image = await Task.Run(() =>
                    {
                        var image = new BitmapImage(new Uri(imageFile.FullName));
                        image.Freeze();
                        return image;
                    });

                    usedImages.Add(imageFile);
                }

                foreach (var image in images.Where(x => usedImages.All(y => y.FullName != x.FullName)))
                    image.Delete();
            }
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