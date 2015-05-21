using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Hurricane.Model;
using Hurricane.Model.Music.Playlist;
using Hurricane.Utilities;
using Hurricane.ViewModel.MainView;
using CollectionView = Hurricane.ViewModel.MainView.CollectionView;

namespace Hurricane.ViewModel
{
    public class NormalViewModel : PropertyChangedBase
    {
        private IViewItem _selectedViewItem;
        private ObservableCollection<IViewItem> _viewItems;
         
        public NormalViewModel()
        {
            var playlist1 = new UserPlaylist {Name = "Beste Musik eva"};
            _viewItems = new ObservableCollection<IViewItem> {new HomeView {IsPlaying = true}, new CollectionView(), new PlaylistView(playlist1)};
            
            ViewItems = CollectionViewSource.GetDefaultView(_viewItems);
            ViewItems.GroupDescriptions.Add(new PropertyGroupDescription("ViewCategorie"));
        }

        public IViewItem SelectedViewItem
        {
            get { return _selectedViewItem; }
            set
            {
                if (SetProperty(value, ref _selectedViewItem))
                    value.Load().Forget();
            }
        }

        public ICollectionView ViewItems { get; set; }
    }
}