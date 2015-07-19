using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.Lastfm
{
    class OpensearchQuery
    {
        [JsonProperty("#text")]
        public string text { get; set; }
        public string role { get; set; }
        public string searchTerms { get; set; }
        public string startPage { get; set; }
    }
}
