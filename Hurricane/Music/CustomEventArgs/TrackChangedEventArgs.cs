using System;
using Hurricane.Music.Track;

namespace Hurricane.Music.CustomEventArgs
{
    public class TrackChangedEventArgs : EventArgs
    {
        public PlayableBase NewTrack { get; protected set; }

        public TrackChangedEventArgs(PlayableBase newtrack)
        {
            NewTrack = newtrack;
        }
    }
}
