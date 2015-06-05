using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.MusicBrainz.GetArtistByTrackId
{
    class Artist
    {
        public string disambiguation { get; set; }
        [JsonProperty("sort-name")]
        public string sortName { get; set; }
        public string name { get; set; }
        public string id { get; set; }
    }
}