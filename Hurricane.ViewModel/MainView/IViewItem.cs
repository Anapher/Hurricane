using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media;
using Hurricane.Model.Music;

namespace Hurricane.ViewModel.MainView
{
    public interface IViewItem : INotifyPropertyChanged
    {
        ViewCategorie ViewCategorie { get; }
        Geometry Icon { get; }
        string Text { get; }
        bool IsPlaying { get; set; }
        Task Load(MusicDataManager musicDataManager);
    }

    public enum ViewCategorie
    {
        Discover,
        MyMusic,
        Playlist
    }
}