using CSCore.SoundOut;

namespace Hurricane.Music.MusicDatabase.EventArgs
{
    public class PlayStateChangedEventArgs : System.EventArgs
    {
        public PlaybackState NewPlaybackState { get; protected set; }

        public PlayStateChangedEventArgs(PlaybackState newstate)
        {
            NewPlaybackState = newstate;
        }
    }
}
