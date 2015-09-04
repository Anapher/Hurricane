using System.Collections.Generic;
using Exceptionless.Json;

namespace Hurricane.Music.MusicCover.APIs.Lastfm
{
    // ReSharper disable InconsistentNaming
    public class OpensearchQuery
    {
        [JsonProperty("#text")]
        public string text { get; set; }
        public string role { get; set; }
        public string startPage { get; set; }
    }

    public class Image
    {
        [JsonProperty("#text")]
        public string text { get; set; }
        public string size { get; set; }
    }

    public class Track
    {
        public string name { get; set; }
        public string artist { get; set; }
        public string url { get; set; }
        public string streamable { get; set; }
        public string listeners { get; set; }
        public List<Image> image { get; set; }
        public string mbid { get; set; }
    }

    public class Trackmatches
    {
        public List<Track> track { get; set; }
    }

    public class Attr
    {
    }

    public class Results
    {
        public Trackmatches trackmatches { get; set; }
    }

    public class LfmSearchResult
    {
        public Results results { get; set; }
    }
}