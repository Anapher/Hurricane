using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Hurricane.Model.Data;
using Hurricane.Model.Music.Playlist;

namespace Hurricane.ViewModel.MainView.Base
{
    public class ViewManager
    {
        public ViewManager(PlaylistProvider playlistProvider)
        {
            ViewItems = new ObservableCollection<ISideListItem>
            {
                new HomeView(),
                new CollectionView(),
                new ChartsView(),
                new QueueView(),
                new HistoryView()
            };

            playlistProvider.PlaylistAdded += PlaylistProvider_PlaylistAdded;
            playlistProvider.PlaylistRemoved += PlaylistProvider_PlaylistRemoved;
            foreach (var userPlaylist in playlistProvider.Playlists)
                ViewItems.Add(new PlaylistView(userPlaylist));

            ViewSource = CollectionViewSource.GetDefaultView(ViewItems);
            ViewSource.GroupDescriptions.Add(new PropertyGroupDescription("ViewCategorie"));
        }

        private void PlaylistProvider_PlaylistAdded(object sender, UserPlaylist e)
        {
            ViewItems.Insert(ViewItems.Count -1, new PlaylistView(e));
        }

        private void PlaylistProvider_PlaylistRemoved(object sender, UserPlaylist e)
        {
            foreach (var viewItem in ViewItems)
            {
                var playlistView = viewItem as PlaylistView;
                if (playlistView == null)
                    continue;
                if (playlistView.Playlist == e)
                {
                    ViewItems.Remove(viewItem);
                    break;
                }
            }
        }

        public ObservableCollection<ISideListItem> ViewItems { get; }
        public ICollectionView ViewSource { get; }
    }
}