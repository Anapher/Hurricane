using Hurricane.Music;
using Hurricane.ViewModelBase;
using Hurricane.ViewModels;
using MahApps.Metro.Controls;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for QueueManager.xaml
    /// </summary>
    public partial class QueueManager : MetroWindow
    {
        public QueueManager()
        {
            InitializeComponent();
        }

        private RelayCommand _movetracksup;
        public RelayCommand MoveTracksUp
        {
            get
            {
                return _movetracksup ?? (_movetracksup = new RelayCommand(parameter =>
                {
                    var manager = MainViewModel.Instance.MusicManager;
                    var selecteditems = lst.SelectedItems;
                    switch (selecteditems.Count)
                    {
                        case 0:
                            return;
                        case 1:
                            manager.Queue.MoveTrackUp(((TrackPlaylistPair)selecteditems[0]).Track);
                            break;
                        default:
                            int startindex = -1;
                            int endindex = 0;

                            foreach (var item in selecteditems) //we search the highest and lowest index
                            {
                                int index = manager.Queue.IndexOf(((TrackPlaylistPair)item).Track);
                                if (startindex == -1) startindex = index;
                                if (index < startindex) { startindex = index; } else if (index > endindex) { endindex = index; }
                            }

                            if (startindex == 0) return;

                            manager.Queue.MoveTrackDown(manager.Queue[startindex - 1].Track, selecteditems.Count);
                            break;
                    }
                }));
            }
        }

        private RelayCommand _movetracksdown;
        public RelayCommand MoveTracksDown
        {
            get
            {
                return _movetracksdown ?? (_movetracksdown = new RelayCommand(parameter =>
                {
                    var manager = MainViewModel.Instance.MusicManager;
                    var selecteditems = lst.SelectedItems;
                    switch (selecteditems.Count)
                    {
                        case 0:
                            return;
                        case 1:
                            manager.Queue.MoveTrackDown(((TrackPlaylistPair)selecteditems[0]).Track);
                            break;
                        default:
                            int startindex = -1;
                            int endindex = 0;

                            foreach (var item in selecteditems) //we search the highest and lowest index
                            {
                                int index = manager.Queue.IndexOf(((TrackPlaylistPair)item).Track);
                                if (startindex == -1) startindex = index;
                                if (index < startindex) { startindex = index; } else if (index > endindex) { endindex = index; }
                            }

                            if (endindex == manager.Queue.Count - 1) return;

                            manager.Queue.MoveTrackUp(manager.Queue[endindex + 1].Track, selecteditems.Count);
                            break;
                    }
                }));
            }
        }
    }
}
