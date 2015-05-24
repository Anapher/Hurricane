using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.Lastfm.GetArtistInfo
{
    class SimilarArtist
    {
        public string name { get; set; }
        public string url { get; set; }
        public List<Image> image { get; set; }
    }
}