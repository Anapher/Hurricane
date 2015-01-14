using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hurricane.Music.Track.YouTubeApi
{
    public class Title
    {
        [JsonProperty("$t")]
        public string Name { get; set; }
    }

    public class Link
    {
        public string rel { get; set; }
        public string type { get; set; }
        public string href { get; set; }
    }

    public class Name
    {
        [JsonProperty("$t")]
        public string Text { get; set; }
    }

    public class Author
    {
        public Name name { get; set; }
    }

    public class Published
    {
        [JsonProperty("$t")]
        public string Date { get; set; }
    }

    public class MediaThumbnail
    {
        public string url { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string time { get; set; }
    }

    public class YtDuration
    {
        public string seconds { get; set; }
    }

    public class YtStatistics
    {
        public string favoriteCount { get; set; }
        public string viewCount { get; set; }
    }

    public class MediaGroup
    {
        [JsonProperty("yt$duration")]
        public YtDuration Duration { get; set; }

        [JsonProperty("media$thumbnail")]
        public List<MediaThumbnail> Thumbnails { get; set; }
    }

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

    public class Feed
    {
        public List<Entry> entry { get; set; }
    }

    public class ApiResult
    {
        public Feed feed { get; set; }
    }
}
