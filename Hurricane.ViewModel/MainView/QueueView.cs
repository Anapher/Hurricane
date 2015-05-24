using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Hurricane.Model.Music;

namespace Hurricane.ViewModel.MainView
{
    public class QueueView : IViewItem
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ViewCategorie ViewCategorie { get; } = ViewCategorie.Discover;
        public Geometry Icon { get; } = (Geometry)Application.Current.Resources["VectorQueue"];
        public string Text => Application.Current.Resources["Queue"].ToString();
        public bool IsPlaying { get; set; }

        public async Task Load(MusicDataManager musicDataManager)
        {
            
        }
    }
}