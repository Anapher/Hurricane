using System.Threading.Tasks;
using System.Windows.Media;

namespace Hurricane.ViewModel.MainView
{
    public class HomeView : IViewItem
    {
        public HomeView()
        {
            Icon =
                Geometry.Parse(
                    "F1 M 20.000,40.000 L 20.000,28.000 L 28.000,28.000 L 28.000,40.000 L 38.000,40.000 L 38.000,24.000 L 44.000,24.000 L 24.000,6.000 L 4.000,24.000 L 10.000,24.000 L 10.000,40.000 L 20.000,40.000 Z");
            ViewCategorie = ViewCategorie.Discover;
        }

        public ViewCategorie ViewCategorie { get; }
        public Geometry Icon { get; }
        public string Text { get; } = "Home";

        public async Task Load()
        {
            
        }
    }
}