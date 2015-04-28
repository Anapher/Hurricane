using System.Collections.ObjectModel;
using Hurricane.Music.View.ListItem;

namespace Hurricane.Music.View
{
    class MusicViewManager
    {
        public ObservableCollection<IListItem> ListItems { get; set; }

        public MusicViewManager()
        {
            ListItems = new ObservableCollection<IListItem>();
            
        }
    }
}
