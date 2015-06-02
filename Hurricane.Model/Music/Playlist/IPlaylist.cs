using System.Collections.Generic;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Music.Playlist
{
    public interface IPlaylist
    {
        IList<IPlayable> Tracks { get; }
    }
}