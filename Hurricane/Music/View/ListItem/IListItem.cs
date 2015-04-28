using System.Windows.Media;

namespace Hurricane.Music.View.ListItem
{
    interface IListItem
    {
        GeometryGroup VectorIcon { get; }
        bool IsPlaying { get; }
        string Name { get; }
        ListItemGroup Group { get; }
    }

    enum ListItemGroup
    {
        Discover,
        YourMusic,
        Playlists
    }
}