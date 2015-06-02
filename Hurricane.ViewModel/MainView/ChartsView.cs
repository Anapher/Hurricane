using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Hurricane.Model;
using Hurricane.Model.DataApi;
using Hurricane.Model.Music;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.Model.Notifications;

namespace Hurricane.ViewModel.MainView
{
    public class ChartsView : PropertyChangedBase, IViewItem
    {
        private List<PreviewTrack> _chartList;
        private bool _isLoading;

        public ViewCategorie ViewCategorie { get; } = ViewCategorie.Discover;
        public Geometry Icon { get; } = (GeometryGroup)Application.Current.Resources["VectorCharts"];
        public string Text => Application.Current.Resources["Charts"].ToString();
        public bool IsPlaying { get; set; }

        public List<PreviewTrack> ChartList
        {
            get { return _chartList; }
            set { SetProperty(value, ref _chartList); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(value, ref _isLoading); }
        }

        public async Task Load(MusicDataManager musicDataManager, NotificationManager notificationManager)
        {
            if (ChartList != null || IsLoading) return;
            IsLoading = true;
            ChartList = await iTunesApi.GetTop100(CultureInfo.CurrentCulture);
            IsLoading = false;
            foreach (var previewTrack in ChartList)
                await previewTrack.Image.DownloadImageAsync();
        }
    }
}