using System.Collections.Generic;
using Hurricane.Music.Track.YouTubeApi.DataClasses;

namespace Hurricane.Music.Track.YouTubeApi
{
    public class YouTubeSearchResult
    {
        public Feed feed { get; set; }

        public class Feed
        {
            public List<Entry> entry { get; set; }
        }
    }
}
