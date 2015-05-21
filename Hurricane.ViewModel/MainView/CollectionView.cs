using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Hurricane.Model;

namespace Hurricane.ViewModel.MainView
{
    public class CollectionView : PropertyChangedBase, IViewItem
    {
        private bool _isPlaying;
        public CollectionView()
        {
            Icon = Geometry.Parse(
                "F1 M 11.627,2.039 L 16.543,20.387 C 16.687,20.922 17.238,21.238 17.760,21.099 L 20.675,20.317 C 21.204,20.176 21.519,19.637 21.373,19.093 L 16.457,0.745 C 16.313,0.210 15.762,-0.107 15.240,0.033 L 12.325,0.814 C 11.796,0.956 11.481,1.495 11.627,2.039 Z M 6.000,1.068 L 6.000,20.063 C 6.000,20.617 6.451,21.066 6.991,21.066 L 10.009,21.066 C 10.556,21.066 11.000,20.626 11.000,20.063 L 11.000,1.068 C 11.000,0.515 10.549,0.066 10.009,0.066 L 6.991,0.066 C 6.444,0.066 6.000,0.505 6.000,1.068 Z M 0.000,1.068 L 0.000,20.063 C 0.000,20.617 0.451,21.066 0.991,21.066 L 4.009,21.066 C 4.556,21.066 5.000,20.626 5.000,20.063 L 5.000,1.068 C 5.000,0.515 4.549,0.066 4.009,0.066 L 0.991,0.066 C 0.444,0.066 0.000,0.505 0.000,1.068 Z");
        }

        public ViewCategorie ViewCategorie { get; } = ViewCategorie.MyMusic;
        public Geometry Icon { get; }
        public string Text { get; } = Application.Current.Resources["Collection"].ToString();

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { SetProperty(value, ref _isPlaying); }
        }

        public async Task Load()
        {

        }
    }
}