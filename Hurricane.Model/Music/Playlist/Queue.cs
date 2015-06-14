using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Music.Playlist
{
    public class Queue : INotifyPropertyChanged
    {
        private bool _isEmpty;

        public Queue()
        {
            QueueItems = new ObservableCollection<QueueItem>();
            QueueItems.CollectionChanged += QueueItems_CollectionChanged;
            IsEmpty = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<QueueItem> QueueItems { get; }
        public bool IsEmpty
        {
            get { return _isEmpty; }
            set
            {
                if (value != _isEmpty)
                {
                    _isEmpty = value;
                    OnPropertyChanged();
                }
            }
        }

        public IPlayable GetNextPlayable()
        {
            if (QueueItems.Count == 0)
                throw new InvalidOperationException();

            var nextPlayable = QueueItems[0];
            QueueItems.RemoveAt(0);
            RefreshIDs();
            return nextPlayable.Playable;
        }

        public void AddTrackToQueue(IPlayable playable)
        {
            QueueItems.Add(new QueueItem {Playable = playable });
        }

        public void AddTrackToQueue(IPlayable playable, TimeSpan duration)
        {
            QueueItems.Add(new QueueItem { Playable = playable, Duration = duration });
        }

        public void AddTrackToQueue(PlayableBase playableBase)
        {
            AddTrackToQueue(playableBase, playableBase.Duration);
        }

        public void RemoveTrackFromQueue(IPlayable playable)
        {
            QueueItems.Remove(QueueItems.First(x => x.Playable == playable));
        }

        private void RefreshIDs()
        {
            for (int i = 0; i < QueueItems.Count; i++)
                QueueItems[i].Number = i + 1;
        }

        private void QueueItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshIDs();
            IsEmpty = QueueItems.Count == 0;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class QueueItem : INotifyPropertyChanged
    {
        private int _number;

        public event PropertyChangedEventHandler PropertyChanged;

        public IPlayable Playable { get; set; }
        public TimeSpan Duration { get; set; }
        public string Artist => Playable.Artist;
        public int Number
        {
            get { return _number; }
            set
            {
                if(_number != value)
                {
                    _number = value;
                    OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}