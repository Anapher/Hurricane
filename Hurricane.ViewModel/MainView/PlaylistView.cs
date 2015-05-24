using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Hurricane.Model;
using Hurricane.Model.Music;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Music.TrackProperties;

namespace Hurricane.ViewModel.MainView
{
    public class PlaylistView : PropertyChangedBase, IViewItem
    {
        private string _searchText;
        private bool _isPlaying;

        public PlaylistView(UserPlaylist playlist)
        {
            Playlist = playlist;
            ViewSource = CollectionViewSource.GetDefaultView(playlist.Tracks);
            ViewSource.Filter = FilterViewSource;

            Icon = new GeometryGroup
            {
                Children =
                    new GeometryCollection
                    {
                        Geometry.Parse(
                            "F1 M 8.000,9.000 L 8.000,15.000 L 15.000,12.000 L 8.000,9.000 Z M 1.000,2.000 L 1.000,3.000 L 21.000,3.000 L 21.000,2.000 L 1.000,2.000 Z M 2.000,0.000 L 2.000,1.000 L 20.000,1.000 L 20.000,0.000 L 2.000,0.000 Z M 0.140,4.000 C 0.063,4.000 0.000,4.058 0.000,4.129 L 0.000,19.871 C 0.000,19.942 0.063,20.000 0.140,20.000 L 21.860,20.000 C 21.937,20.000 22.000,19.942 22.000,19.871 L 22.000,4.129 C 22.000,4.058 21.937,4.000 21.860,4.000 L 0.140,4.000 Z")
                    }
            };
            Artists = new ObservableCollection<Artist>();
        }

        public UserPlaylist Playlist { get; set; }
        public ICollectionView ViewSource { get; }
        public ViewCategorie ViewCategorie { get; } = ViewCategorie.Playlist;
        public ObservableCollection<Artist> Artists { get; set; }
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

        public async Task Load(MusicDataManager musicDataManager)
        {
            Artists.Clear();
            foreach (var artistName in Playlist.Tracks.Select(x => x.Artist).Distinct())
            {
                var artist = await musicDataManager.LastfmApi.SearchArtist(artistName);
                if (!Artists.Contains(artist))
                    Artists.Add(artist);
            }

            foreach (var artist in Artists)
            {
                await artist.MediumImage.LoadImageAsync();
            }
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