using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Hurricane.Model.DataApi;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.ViewModel.MainView.Base;

namespace Hurricane.ViewModel.MainView
{
    public class ChartsView : SideListItem
    {
        private List<PreviewTrack> _chartList;
        private bool _isLoading;

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

        protected async override Task Load()
        {
            IsLoading = true;
            ChartList = await iTunesApi.GetTop100(CultureInfo.CurrentCulture);
            IsLoading = false;

            foreach (var previewTrack in ChartList)
                await previewTrack.Image.LoadImageAsync();
        }
    }
}