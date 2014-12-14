using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Hurricane.ViewModelBase;
using Hurricane.Music;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaktionslogik für QueueManager.xaml
    /// </summary>
    public partial class QueueManager : MahApps.Metro.Controls.MetroWindow
    {
        public QueueManager()
        {
            InitializeComponent();
        }

        private RelayCommand movetracksup;
        public RelayCommand MoveTracksUp
        {
            get
            {
                if (movetracksup == null)
                    movetracksup = new RelayCommand((object parameter) =>
                    {
                        var manager = ViewModels.MainViewModel.Instance.MusicManager;
                        var selecteditems = lst.SelectedItems;
                        if (selecteditems.Count == 0) return;
                        if (selecteditems.Count == 1)
                        {
                            manager.Queue.MoveTrackUp(((TrackPlaylistPair)selecteditems[0]).Track);
                        }
                        else
                        {
                            int startindex = -1;
                            int endindex = 0;

                            foreach (var item in selecteditems) //we search the highest and lowest index
                            {
                                int index =  manager.Queue.IndexOf(((TrackPlaylistPair)item).Track);
                                if (startindex == -1) startindex = index;
                                if (index < startindex) { startindex = index; } else if (index > endindex) { endindex = index; }
                            }

                            if (startindex == 0) return;

                            manager.Queue.MoveTrackDown(manager.Queue[startindex -1].Track, selecteditems.Count);
                        }
                    });
                return movetracksup;
            }
        }

        private RelayCommand movetracksdown;
        public RelayCommand MoveTracksDown
        {
            get
            {
                if (movetracksdown == null)
                    movetracksdown = new RelayCommand((object parameter) =>
                    {
                        var manager = ViewModels.MainViewModel.Instance.MusicManager;
                        var selecteditems = lst.SelectedItems;
                        if (selecteditems.Count == 0) return;
                        if (selecteditems.Count == 1)
                        {
                            manager.Queue.MoveTrackDown(((TrackPlaylistPair)selecteditems[0]).Track);
                        }
                        else
                        {
                            int startindex = -1;
                            int endindex = 0;

                            foreach (var item in selecteditems) //we search the highest and lowest index
                            {
                                int index = manager.Queue.IndexOf(((TrackPlaylistPair)item).Track);
                                if (startindex == -1) startindex = index;
                                if (index < startindex) { startindex = index; } else if (index > endindex) { endindex = index; }
                            }

                            if (endindex == manager.Queue.Count -1) return;

                            manager.Queue.MoveTrackUp(manager.Queue[endindex + 1].Track, selecteditems.Count);
                        }
                    });
                return movetracksdown;
            }
        }
    }
}
