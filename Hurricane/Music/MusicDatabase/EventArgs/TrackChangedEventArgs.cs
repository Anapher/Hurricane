namespace Hurricane.Music.MusicDatabase.EventArgs
{
    public class TrackChangedEventArgs : System.EventArgs
    {
        public Track NewTrack { get; protected set; }

        public TrackChangedEventArgs(Track newtrack)
        {
            NewTrack = newtrack;
        }
    }
}
