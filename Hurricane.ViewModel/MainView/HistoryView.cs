using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Hurricane.Model.Music.Playlist;
using Hurricane.ViewModel.MainView.Base;
using TaskExtensions = Hurricane.Utilities.TaskExtensions;

namespace Hurricane.ViewModel.MainView
{
    public class HistoryView : SideListItem
    {
        private History _history;

        public override ViewCategorie ViewCategorie { get; } = ViewCategorie.MyMusic;
        public override Geometry Icon { get; } = (Geometry) Application.Current.Resources["VectorClock"];
        public override string Text => Application.Current.Resources["History"].ToString();

        public History History
        {
            get { return _history; }
            set { SetProperty(value, ref _history); }
        }

        protected override Task Load()
        {
            History = MusicDataManager.UserData.UserData.History;
            return TaskExtensions.CompletedTask;
        }
    }
}