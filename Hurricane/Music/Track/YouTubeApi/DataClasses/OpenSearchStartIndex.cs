using Newtonsoft.Json;

namespace Hurricane.Music.Track.YouTubeApi.DataClasses
{
    public class OpenSearchStartIndex
    {
        [JsonProperty("$t")]
        public int Number { get; set; }
    }
}
