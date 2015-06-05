using Newtonsoft.Json;

namespace Hurricane.Model.DataApi.SerializeClasses.Lastfm.SearchTrack
{
    class Results
    {
        [JsonProperty("opensearch:Query")]
        public OpensearchQuery OpensearchQuery { get; set; }
        [JsonProperty("opensearch:totalResults")]
        public string OpensearchTotalResults { get; set; }
        [JsonProperty("opensearch:startIndex")]
        public string OpensearchStartIndex { get; set; }
        [JsonProperty("opensearch:itemsPerPage")]
        public string OpensearchItemsPerPage { get; set; }
        [JsonProperty("trackmatches")]
        public Trackmatches Trackmatches { get; set; }
    }
}
