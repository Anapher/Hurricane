using Hurricane.Music.Playlist;
using Hurricane.Music.Track;

namespace Hurricane.Music.Data
{
    public class TrackPlaylistPair
    {
        public PlayableBase Track { get; set; }
        public IPlaylist Playlist { get; set; }

        public TrackPlaylistPair(PlayableBase track, IPlaylist playlist)
        {
            Track = track;
            Playlist = playlist;
        }
    }
}
