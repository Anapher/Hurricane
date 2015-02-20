using System.Xml.Serialization;

namespace Hurricane.Music.MusicDatabase
{
    public partial class lfm
    {
        /// <remarks/>
        public lfmTrack track { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmTrack
    {
        /// <remarks/>
        public long id { get; set; }

        /// <remarks/>
        public string name { get; set; }

        /// <remarks/>
        public object mbid { get; set; }

        /// <remarks/>
        public string url { get; set; }

        /// <remarks/>
        public long duration { get; set; }

        /// <remarks/>
        public lfmTrackStreamable streamable { get; set; }

        /// <remarks/>
        public lfmTrackArtist artist { get; set; }

        /// <remarks/>
        public lfmTrackAlbum album { get; set; }

        /// <remarks/>
        [XmlArrayItem("tag", IsNullable = false)]
        public lfmTrackTag[] toptags { get; set; }

        /// <remarks/>
        public lfmTrackWiki wiki { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmTrackStreamable
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
    public partial class lfmTrackArtist
    {
        /// <remarks/>
        public string name { get; set; }

        /// <remarks/>
        public string mbid { get; set; }

        /// <remarks/>
        public string url { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmTrackAlbum
    {
        /// <remarks/>
        public string artist { get; set; }

        /// <remarks/>
        public string title { get; set; }

        /// <remarks/>
        public string mbid { get; set; }

        /// <remarks/>
        public string url { get; set; }

        /// <remarks/>
        [XmlElement("image")]
        public lfmTrackAlbumImage[] image { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public byte position { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmTrackAlbumImage
    {
        /// <remarks/>
        [XmlAttribute()]
        public string size { get; set; }

        /// <remarks/>
        [XmlText()]
        public string Value { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmTrackTag
    {
        /// <remarks/>
        public string name { get; set; }

        /// <remarks/>
        public string url { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmTrackWiki
    {
        /// <remarks/>
        public string published { get; set; }

        /// <remarks/>
        public string summary { get; set; }

        /// <remarks/>
        public string content { get; set; }
    }
}
