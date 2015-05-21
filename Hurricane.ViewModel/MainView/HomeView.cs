using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Hurricane.Model;
using Hurricane.ViewModel.Annotations;

namespace Hurricane.ViewModel.MainView
{
    public class HomeView : PropertyChangedBase, IViewItem
    {
        private bool _isPlaying;

        public HomeView()
        {
            Icon =
                Geometry.Parse(
                    "F1 M 41.709,22.300 L 21.709,0.300 C 21.309,-0.100 20.609,-0.100 20.209,0.300 L 0.209,22.300 C 0.009,22.600 -0.091,23.000 0.109,23.400 C 0.209,23.800 0.609,24.000 1.009,24.000 L 6.009,24.000 L 6.009,43.000 C 6.009,43.600 6.409,44.000 7.009,44.000 L 16.009,44.000 C 16.609,44.000 17.009,43.600 17.009,43.000 L 17.009,33.000 L 25.009,33.000 L 25.009,43.000 C 25.009,43.600 25.409,44.000 26.009,44.000 L 35.009,44.000 C 35.609,44.000 36.009,43.600 36.009,43.000 L 36.009,24.000 L 41.009,24.000 C 41.409,24.000 41.809,23.800 41.909,23.400 C 42.109,23.000 42.009,22.600 41.709,22.300 Z");
        }

        public ViewCategorie ViewCategorie { get; } = ViewCategorie.Discover;
        public Geometry Icon { get; }
        public string Text { get; } = Application.Current.Resources["Home"].ToString();

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