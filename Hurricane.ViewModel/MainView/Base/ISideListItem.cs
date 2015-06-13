using System.Windows.Media;

namespace Hurricane.ViewModel.MainView.Base
{
    public interface ISideListItem : IViewItem
    {
        ViewCategorie ViewCategorie { get; }
        Geometry Icon { get; }
        string Text { get; }

    }

    public enum ViewCategorie
    {
        Discover,
        MyMusic,
        Playlist
    }
}
