using Newtonsoft.Json;

namespace Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses
{
    public class Title
    {
        [JsonProperty("$t")]
        public string Name { get; set; }
    }
}
