using System;
using System.Collections.ObjectModel;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Music.Playlist
{
    public class Queue
    {
        public Queue()
        {
            Playables = new ObservableCollection<IPlayable>();
        }

        public ObservableCollection<IPlayable> Playables { get; }

        public IPlayable GetNextPlayable()
        {
            if (Playables.Count == 0)
                throw new InvalidOperationException();

            var nextPlayable = Playables[0];
            Playables.RemoveAt(0);
            return nextPlayable;
        }

        public void AddTrackToQueue(IPlayable playable)
        {
            Playables.Add(playable);
        }
    }
}