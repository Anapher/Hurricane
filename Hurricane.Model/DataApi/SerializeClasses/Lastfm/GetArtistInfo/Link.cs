using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.Lastfm.GetArtistInfo
{
    class Link
    {
        [JsonProperty("#text")]
        public string text { get; set; }
        public string rel { get; set; }
        public string href { get; set; }
    }
}
