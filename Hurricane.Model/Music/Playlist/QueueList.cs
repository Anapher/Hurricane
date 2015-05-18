using System.Collections.Generic;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Music.Playlist
{
    public class QueueList : IPlaylist
    {
        public IEnumerable<IPlayable> Tracks { get; }
    }
}