using System.Collections.Generic;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Music.Playlist
{
    public class QueueList : IPlaylist
    {
        public IList<IPlayable> Tracks { get; }
        public IList<IPlayable> History { get; }
    }
}