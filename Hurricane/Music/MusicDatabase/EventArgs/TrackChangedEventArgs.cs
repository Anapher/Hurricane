using Hurricane.Music.Track;

namespace Hurricane.Music.MusicDatabase.EventArgs
{
    public class TrackChangedEventArgs : System.EventArgs
    {
        public PlayableBase NewTrack { get; protected set; }

        public TrackChangedEventArgs(PlayableBase newtrack)
        {
            NewTrack = newtrack;
        }
    }
}
