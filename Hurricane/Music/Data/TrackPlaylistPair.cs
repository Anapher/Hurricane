namespace Hurricane.Music.Data
{
    public class TrackPlaylistPair
    {
        public Track Track { get; set; }
        public IPlaylist Playlist { get; set; }

        public TrackPlaylistPair(Track track, IPlaylist playlist)
        {
            Track = track;
            Playlist = playlist;
        }
    }
}
