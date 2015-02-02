using System.Xml.Serialization;

namespace Hurricane.Music.MusicDatabase
{
    public partial class lfm
    {
        /// <remarks/>
        public lfmResults results { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string status { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmResults
    {
        /// <remarks/>
        [XmlElement(Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public Query Query { get; set; }

        /// <remarks/>
        [XmlElement(Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public uint totalResults { get; set; }

        /// <remarks/>
        [XmlElement(Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public byte startIndex { get; set; }

        /// <remarks/>
        [XmlElement(Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public byte itemsPerPage { get; set; }

        /// <remarks/>
        [XmlArrayItem("track", IsNullable = false)]
        public lfmResultsTrack[] trackmatches { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string @for { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
    [XmlRoot(Namespace = "http://a9.com/-/spec/opensearch/1.1/", IsNullable = false)]
    public partial class Query
    {
        /// <remarks/>
        [XmlAttribute()]
        public string role { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string searchTerms { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public byte startPage { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmResultsTrack
    {
        /// <remarks/>
        public string name { get; set; }

        /// <remarks/>
        public string artist { get; set; }

        /// <remarks/>
        public string url { get; set; }

        /// <remarks/>
        public lfmResultsTrackStreamable streamable { get; set; }

        /// <remarks/>
        [XmlElement("image")]
        public lfmResultsTrackImage[] image { get; set; }

        /// <remarks/>
        public string mbid { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmResultsTrackStreamable
    {
        /// <remarks/>
        [XmlAttribute()]
        public byte fulltrack { get; set; }

        /// <remarks/>
        [XmlText()]
        public byte Value { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmResultsTrackImage
    {
        /// <remarks/>
        [XmlAttribute()]
        public string size { get; set; }

        /// <remarks/>
        [XmlText()]
        public string Value { get; set; }
    }


}
