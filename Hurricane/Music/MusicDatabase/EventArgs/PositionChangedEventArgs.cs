namespace Hurricane.Music.MusicDatabase.EventArgs
{
    public class PositionChangedEventArgs : System.EventArgs
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
