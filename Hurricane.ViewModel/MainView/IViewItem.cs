using System.Threading.Tasks;
using System.Windows.Media;

namespace Hurricane.ViewModel.MainView
{
    public interface IViewItem
    {
        ViewCategorie ViewCategorie { get; }
        Geometry Icon { get; }
        string Text { get; }
        Task Load();
    }

    public enum ViewCategorie
    {
        Discover,
        MyMusic,
        Playlist
    }
}