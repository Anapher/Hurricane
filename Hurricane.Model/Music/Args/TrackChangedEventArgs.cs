using System;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Music.Args
{
    public class TrackChangedEventArgs
    {
        public TrackChangedEventArgs(IPlayable track, TimeSpan timePlayed)
        {
            Track = track;
            TimePlayed = timePlayed;
        }

        public IPlayable Track { get; }
        public TimeSpan TimePlayed { get; }
    }
}