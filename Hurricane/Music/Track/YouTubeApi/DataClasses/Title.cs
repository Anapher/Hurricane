using Newtonsoft.Json;

namespace Hurricane.Music.Track.YouTubeApi.DataClasses
{
    public class Title
    {
        [JsonProperty("$t")]
        public string Name { get; set; }
    }
}
