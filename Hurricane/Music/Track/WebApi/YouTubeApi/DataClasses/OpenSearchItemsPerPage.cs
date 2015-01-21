using Newtonsoft.Json;

namespace Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses
{
    public class OpenSearchItemsPerPage
    {
        [JsonProperty("$t")]
        public int Number { get; set; }
    }
}
