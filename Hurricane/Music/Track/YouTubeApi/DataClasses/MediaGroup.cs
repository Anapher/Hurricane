using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hurricane.Music.Track.YouTubeApi.DataClasses
{
    public class MediaGroup
    {
        [JsonProperty("yt$duration")]
        public YtDuration Duration { get; set; }

        [JsonProperty("media$thumbnail")]
        public List<MediaThumbnail> Thumbnails { get; set; }

        [JsonProperty("media$content")]
        public List<MediaContent> Content { get; set; }

        [JsonProperty("media$title")]
        public MediaTitle Title { get; set; }
    }
}
