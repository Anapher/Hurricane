using System;

namespace Hurricane.Music.CustomEventArgs
{
    public class PositionChangedEventArgs : EventArgs
    {
        public int NewPosition { get; protected set; }
        public int TrackLength { get; protected set; }

        public PositionChangedEventArgs(int newposition, int tracklength)
        {
            NewPosition = newposition;
            TrackLength = tracklength;
        }
    }
}
