using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Hurricane.Model;
using Hurricane.Model.Music;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using TaskExtensions = Hurricane.Utilities.TaskExtensions;

namespace Hurricane.ViewModel.MainView
{
    public class PlaylistView : PropertyChangedBase, IViewItem
    {
        private string _searchText;
        private bool _isPlaying;
        private bool _isLoaded;

        public PlaylistView(UserPlaylist playlist)
        {
            Playlist = playlist;

            Icon = new GeometryGroup
            {
                Children =
                    new GeometryCollection
                    {
                        Geometry.Parse(
                            "F1 M 8.000,9.000 L 8.000,15.000 L 15.000,12.000 L 8.000,9.000 Z M 1.000,2.000 L 1.000,3.000 L 21.000,3.000 L 21.000,2.000 L 1.000,2.000 Z M 2.000,0.000 L 2.000,1.000 L 20.000,1.000 L 20.000,0.000 L 2.000,0.000 Z M 0.140,4.000 C 0.063,4.000 0.000,4.058 0.000,4.129 L 0.000,19.871 C 0.000,19.942 0.063,20.000 0.140,20.000 L 21.860,20.000 C 21.937,20.000 22.000,19.942 22.000,19.871 L 22.000,4.129 C 22.000,4.058 21.937,4.000 21.860,4.000 L 0.140,4.000 Z")
                    }
            };
        }

        public UserPlaylist Playlist { get; }
        public ICollectionView ViewSource { get; private set; }
        public ViewCategorie ViewCategorie { get; } = ViewCategorie.Playlist;
        public ICollectionView ArtistViewSource { get; set; }
        public string Text => Playlist.Name;
        public Geometry Icon { get; }
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { SetProperty(value, ref _isPlaying); }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SetProperty(value, ref _searchText))
                    ViewSource.Refresh();
            }
        }

        public Task Load(MusicDataManager musicDataManager)
        {
            if (!_isLoaded)
            {
                ViewSource = CollectionViewSource.GetDefaultView(Playlist.Tracks);
                ViewSource.Filter = FilterViewSource;

                ArtistViewSource = CollectionViewSource.GetDefaultView(Playlist.Tracks);
                ArtistViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Artist"));
                _isLoaded = true;
            }

            return TaskExtensions.CompletedTask;
        }

        private bool FilterViewSource(object o)
        {
            var track = (IPlayable)o;
            return string.IsNullOrWhiteSpace(_searchText) ||
                   track.Title.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) > -1 ||
                   track.Artist.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) > -1;
        }
    }
}