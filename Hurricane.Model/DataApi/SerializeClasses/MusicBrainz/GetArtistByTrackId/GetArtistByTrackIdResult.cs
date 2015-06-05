using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.MusicBrainz.GetArtistByTrackId
{
    class GetArtistByTrackIdResult
    {
        public string disambiguation { get; set; }
        public object length { get; set; }
        public int video { get; set; }
        [JsonProperty("artist-credit")]
        public List<ArtistCredit> artistCredits { get; set; }
        public string id { get; set; }
        public string title { get; set; }
    }
}