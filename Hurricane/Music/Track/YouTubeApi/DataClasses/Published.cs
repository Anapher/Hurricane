using Newtonsoft.Json;

namespace Hurricane.Music.Track.YouTubeApi.DataClasses
{
    public class Published
    {
        [JsonProperty("$t")]
        public string Date { get; set; }
    }
}
