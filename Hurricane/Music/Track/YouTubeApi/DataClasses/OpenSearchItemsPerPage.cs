using Newtonsoft.Json;

namespace Hurricane.Music.Track.YouTubeApi.DataClasses
{
    public class OpenSearchItemsPerPage
    {
        [JsonProperty("$t")]
        public int Number { get; set; }
    }
}
