using System.Collections.Generic;
using Newtonsoft.Json;
using Hurricane.Music.Track.YouTubeApi.DataClasses;

namespace Hurricane.Music.Track.YouTubeApi
{
    public class YouTubePlaylistResult
    {
        public Feed feed { get; set; }

        public class Feed
        {
            public string xmlns { get; set; }
            public Title title { get; set; }
            public List<Link> link { get; set; }
            public List<Author> author { get; set; }

            [JsonProperty("openSearch$totalResults")]
            public OpenSearchTotalResults SearchTotalResults { get; set; }
            [JsonProperty("openSearch$startIndex")]
            public OpenSearchStartIndex SearchStartIndex { get; set; }
            [JsonProperty("openSearch$itemsPerPage")]
            public OpenSearchItemsPerPage SearchItemsPerPage { get; set; }
            [JsonProperty("media$group")]
            public MediaGroup MediaGroup { get; set; }

            public List<Entry> entry { get; set; }
        }
    }
}
