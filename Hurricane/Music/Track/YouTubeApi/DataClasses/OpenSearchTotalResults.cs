using Newtonsoft.Json;

namespace Hurricane.Music.Track.YouTubeApi.DataClasses
{
    public class OpenSearchTotalResults
    {
        [JsonProperty("$t")]
        public int Number { get; set; }
    }
}
