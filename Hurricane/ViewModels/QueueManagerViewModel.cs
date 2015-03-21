using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurricane.Music;
using Hurricane.Music.Data;
using Hurricane.ViewModelBase;

namespace Hurricane.ViewModels
{
    public class QueueManagerViewModel
    {
        public QueueManagerViewModel(QueueManager queueManager)
        {
            QueueManager = queueManager;
        }

        public QueueManager QueueManager { get; set; }

        private RelayCommand _movetracksup;
        public RelayCommand MoveTracksUp
        {
            get
            {
                return _movetracksup ?? (_movetracksup = new RelayCommand(parameter =>
                {
                    var selecteditems = ((IList)parameter).Cast<TrackPlaylistPair>().ToList();
                    switch (selecteditems.Count)
                    {
                        case 0:
                            return;
                        case 1:
                            QueueManager.MoveTrackUp((selecteditems[0]).Track);
                            break;
                        default:
                            int startindex = -1;
                            int endindex = 0;

                            foreach (var item in selecteditems) //we search the highest and lowest index
                            {
                                int index = QueueManager.IndexOf((item).Track);
                                if (startindex == -1) startindex = index;
                                if (index < startindex) { startindex = index; } else if (index > endindex) { endindex = index; }
                            }

                            if (startindex == 0) return;

                            QueueManager.MoveTrackDown(QueueManager[startindex - 1].Track, selecteditems.Count);
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
                    var selecteditems = ((IList)parameter).Cast<TrackPlaylistPair>().ToList();
                    switch (selecteditems.Count)
                    {
                        case 0:
                            return;
                        case 1:
                            QueueManager.MoveTrackDown((selecteditems[0]).Track);
                            break;
                        default:
                            int startindex = -1;
                            int endindex = 0;

                            foreach (var item in selecteditems) //we search the highest and lowest index
                            {
                                int index = QueueManager.IndexOf((item).Track);
                                if (startindex == -1) startindex = index;
                                if (index < startindex) { startindex = index; } else if (index > endindex) { endindex = index; }
                            }

                            if (endindex == QueueManager.Count - 1) return;

                            QueueManager.MoveTrackUp(QueueManager[endindex + 1].Track, selecteditems.Count);
                            break;
                    }
                }));
            }
        }

        private RelayCommand _removeSelectedTracksFromQueue;
        public RelayCommand RemoveSelectedTracksFromQueue
        {
            get
            {
                return _removeSelectedTracksFromQueue ?? (_removeSelectedTracksFromQueue = new RelayCommand(parameter =>
                {
                    var selecteditems = ((IList)parameter).Cast<TrackPlaylistPair>().ToList();
                    if (selecteditems.Count == 0) return;

                    foreach (var trackPlaylistPair in selecteditems)
                    {
                        QueueManager.RemoveTrack(trackPlaylistPair.Track);
                    }
                }));
            }
        }

        private RelayCommand _clearQueue;
        public RelayCommand ClearQueue
        {
            get
            {
                return _clearQueue ?? (_clearQueue = new RelayCommand(parameter =>
                {
                    QueueManager.ClearTracks();
                }));
            }
        }
    }
}
