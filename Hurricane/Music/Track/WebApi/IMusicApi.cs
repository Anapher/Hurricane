using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hurricane.Music.Track.WebApi
{
    interface IMusicApi
    {
        Task<Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>> CheckForSpecialUrl(string url);
        string ServiceName { get; }
        Task<List<WebTrackResultBase>> Search(string searchText);
    }
}
