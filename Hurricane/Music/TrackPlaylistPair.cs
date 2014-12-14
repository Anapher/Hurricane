using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music
{
    public class TrackPlaylistPair
    {
        public Track Track { get; set; }
        public Playlist Playlist { get; set; }

        public TrackPlaylistPair(Track track, Playlist playlist)
        {
            this.Track = track;
            this.Playlist = playlist;
        }
    }
}
