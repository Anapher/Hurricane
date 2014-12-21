using CSCore.SoundOut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music
{
    public class PlayStateChangedEventArgs : EventArgs
    {
        public PlaybackState NewPlaybackState { get; protected set; }

        public PlayStateChangedEventArgs(PlaybackState newstate)
        {
            this.NewPlaybackState = newstate;
        }
    }
}
