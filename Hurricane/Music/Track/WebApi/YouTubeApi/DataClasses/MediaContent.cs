using Newtonsoft.Json;

namespace Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses
{
    public class MediaContent
    {
        public string url { get; set; }
        public string type { get; set; }
        [JsonProperty("yt$format")]
        public int Format { get; set; }
    }
}
