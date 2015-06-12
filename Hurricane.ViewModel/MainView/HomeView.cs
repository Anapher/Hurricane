using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Hurricane.Model;
using Hurricane.Model.Music;
using Hurricane.Model.Notifications;

namespace Hurricane.ViewModel.MainView
{
    public class HomeView : PropertyChangedBase, IViewItem
    {
        private bool _isPlaying;

        public ViewCategorie ViewCategorie { get; } = ViewCategorie.Discover;
        public Geometry Icon { get; } = (Geometry)Application.Current.Resources["VectorHome"];
        public string Text => Application.Current.Resources["Home"].ToString();

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { SetProperty(value, ref _isPlaying); }
        }

        public async Task Load(MusicDataManager musicDataManager, ViewController viewController, NotificationManager notificationManager)
        {

        }
    }
}