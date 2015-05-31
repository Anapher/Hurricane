using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Hurricane.Model;
using Hurricane.Model.Music;
using Hurricane.Model.Notifications;

namespace Hurricane.ViewModel.MainView
{
    public class CollectionView : PropertyChangedBase, IViewItem
    {
        private bool _isPlaying;

        public ViewCategorie ViewCategorie { get; } = ViewCategorie.MyMusic;
        public Geometry Icon { get; } = (Geometry) Application.Current.Resources["VectorCollection"];
        public string Text => Application.Current.Resources["Collection"].ToString();

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { SetProperty(value, ref _isPlaying); }
        }

        public async Task Load(MusicDataManager musicDataManager, NotificationManager notificationManager)
        {
            
        }
    }
}