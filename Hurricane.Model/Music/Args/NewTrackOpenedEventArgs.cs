using System;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Music.Args
{
    public class NewTrackOpenedEventArgs : EventArgs
    {
        public NewTrackOpenedEventArgs(IPlayable newTrack)
        {
            NewTrack = newTrack;
        }

        public IPlayable NewTrack { get; set; }
    }
}
