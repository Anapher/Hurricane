using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses
{
    public class Entry
    {
        [JsonProperty("media$group")]
        public MediaGroup MediaGroup { get; set; }
        public Published published { get; set; }
        public Title title { get; set; }
        public List<Link> link { get; set; }
        public List<Author> author { get; set; }
        [JsonProperty("yt$statistics")]
        public YtStatistics Statistics { get; set; }
    }
}
