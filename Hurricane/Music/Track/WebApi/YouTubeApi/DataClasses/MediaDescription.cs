using Newtonsoft.Json;

namespace Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses
{
    public class MediaDescription
    {
        [JsonProperty("$t")]
        public string Text { get; set; }
    }
}
