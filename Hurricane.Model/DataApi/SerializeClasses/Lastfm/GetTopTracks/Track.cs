using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.Lastfm.GetTopTracks
{
    class Track
    {
        public string name { get; set; }
        public string duration { get; set; }
        public string playcount { get; set; }
        public string listeners { get; set; }
        public string mbid { get; set; }
        public string url { get; set; }
        public Streamable streamable { get; set; }
        public Artist artist { get; set; }
        public List<Image> image { get; set; }
    }
}