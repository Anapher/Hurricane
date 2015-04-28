using System.Windows;
using System.Windows.Media;
using Hurricane.Music.Playlist;
using Hurricane.ViewModelBase;

namespace Hurricane.Music.View.ListItem
{
    class PlaylistItem : PropertyChangedBase, IListItem
    {
        public NormalPlaylist Playlist { get; set; }

        private static GeometryGroup _vectorIcon;
        public GeometryGroup VectorIcon
        {
            get
            {
                return _vectorIcon ?? (_vectorIcon = (GeometryGroup) Application.Current.Resources["VectorPlaylist"]);
            }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { SetProperty(value, ref _isPlaying); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(value, ref _name); }
        }

        public string Description
        {
            get { return Application.Current.Resources["PlaylistDetails"].ToString(); }
        }

        public ListItemGroup Group
        {
            get { return ListItemGroup.Playlists; }
        }

        public PlaylistItem(NormalPlaylist playlist)
        {
            Playlist = playlist;
            Name = Playlist.Name;
            Playlist.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Name")
                    Name = Playlist.Name;
            };
        }
    }
}