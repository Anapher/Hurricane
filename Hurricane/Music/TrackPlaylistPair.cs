namespace Hurricane.Music
{
    public class TrackPlaylistPair
    {
        public Track Track { get; set; }
        public Playlist Playlist { get; set; }

        public TrackPlaylistPair(Track track, Playlist playlist)
        {
            Track = track;
            Playlist = playlist;
        }
    }
}
