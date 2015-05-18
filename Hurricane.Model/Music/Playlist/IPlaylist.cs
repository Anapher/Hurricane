using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Music.Playlist
{
    public interface IPlaylist
    {
        IEnumerable<IPlayable> Tracks { get; }

    }
}