using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.Lastfm.SearchArtist
{
    class Artist
    {
        public string name { get; set; }
        public string listeners { get; set; }
        public string mbid { get; set; }
        public string url { get; set; }
        public string streamable { get; set; }
        public List<Image> image { get; set; }
    }
}
