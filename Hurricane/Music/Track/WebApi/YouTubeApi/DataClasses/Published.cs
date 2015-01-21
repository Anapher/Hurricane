using Newtonsoft.Json;

namespace Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses
{
    public class Published
    {
        [JsonProperty("$t")]
        public string Date { get; set; }
    }
}
