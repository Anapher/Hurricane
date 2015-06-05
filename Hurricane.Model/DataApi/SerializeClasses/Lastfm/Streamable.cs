using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.Lastfm
{
    class Streamable
    {
        [JsonProperty("#text")]
        public string text { get; set; }
        public string fulltrack { get; set; }
    }
}
