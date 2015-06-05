using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.Lastfm.SearchTrack
{
    class Track
    {
        public string name { get; set; }
        public string artist { get; set; }
        public string url { get; set; }
        public Streamable streamable { get; set; }
        public string listeners { get; set; }
        public List<Image> image { get; set; }
        public string mbid { get; set; }
    }
}
