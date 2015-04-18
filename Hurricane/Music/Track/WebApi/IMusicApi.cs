using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace Hurricane.Music.Track.WebApi
{
    public interface IMusicApi
    {
        Task<Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>> CheckForSpecialUrl(string url);
        string ServiceName { get; }
        bool IsEnabled { get; }
        Task<List<WebTrackResultBase>> Search(string searchText);
        FrameworkElement ApiSettings { get; }
    }
}