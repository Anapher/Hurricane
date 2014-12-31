using System.Xml.Serialization;

namespace Hurricane.Music.MusicDatabase
{
    public partial class lfm
    {

        private lfmTrack trackField;

        /// <remarks/>
        public lfmTrack track
        {
            get
            {
                return this.trackField;
            }
            set
            {
                this.trackField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmTrack
    {

        private long idField;

        private string nameField;

        private object mbidField;

        private string urlField;

        private long durationField;

        private lfmTrackStreamable streamableField;

        private long playcountField;

        private lfmTrackArtist artistField;

        private lfmTrackAlbum albumField;

        private lfmTrackTag[] toptagsField;

        private lfmTrackWiki wikiField;

        /// <remarks/>
        public long id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public object mbid
        {
            get
            {
                return this.mbidField;
            }
            set
            {
                this.mbidField = value;
            }
        }

        /// <remarks/>
        public string url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }

        /// <remarks/>
        public long duration
        {
            get
            {
                return this.durationField;
            }
            set
            {
                this.durationField = value;
            }
        }

        /// <remarks/>
        public lfmTrackStreamable streamable
        {
            get
            {
                return this.streamableField;
            }
            set
            {
                this.streamableField = value;
            }
        }

        /// <remarks/>
        public long playcount
        {
            get
            {
                return this.playcountField;
            }
            set
            {
                this.playcountField = value;
            }
        }

        /// <remarks/>
        public lfmTrackArtist artist
        {
            get
            {
                return this.artistField;
            }
            set
            {
                this.artistField = value;
            }
        }

        /// <remarks/>
        public lfmTrackAlbum album
        {
            get
            {
                return this.albumField;
            }
            set
            {
                this.albumField = value;
            }
        }

        /// <remarks/>
        [XmlArrayItem("tag", IsNullable = false)]
        public lfmTrackTag[] toptags
        {
            get
            {
                return this.toptagsField;
            }
            set
            {
                this.toptagsField = value;
            }
        }

        /// <remarks/>
        public lfmTrackWiki wiki
        {
            get
            {
                return this.wikiField;
            }
            set
            {
                this.wikiField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmTrackStreamable
    {

        private byte fulltrackField;

        private byte valueField;

        /// <remarks/>
        [XmlAttribute()]
        public byte fulltrack
        {
            get
            {
                return this.fulltrackField;
            }
            set
            {
                this.fulltrackField = value;
            }
        }

        /// <remarks/>
        [XmlText()]
        public byte Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmTrackArtist
    {

        private string nameField;

        private string mbidField;

        private string urlField;

        /// <remarks/>
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public string mbid
        {
            get
            {
                return this.mbidField;
            }
            set
            {
                this.mbidField = value;
            }
        }

        /// <remarks/>
        public string url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmTrackAlbum
    {

        private string artistField;

        private string titleField;

        private string mbidField;

        private string urlField;

        private lfmTrackAlbumImage[] imageField;

        private byte positionField;

        /// <remarks/>
        public string artist
        {
            get
            {
                return this.artistField;
            }
            set
            {
                this.artistField = value;
            }
        }

        /// <remarks/>
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        public string mbid
        {
            get
            {
                return this.mbidField;
            }
            set
            {
                this.mbidField = value;
            }
        }

        /// <remarks/>
        public string url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }

        /// <remarks/>
        [XmlElement("image")]
        public lfmTrackAlbumImage[] image
        {
            get
            {
                return this.imageField;
            }
            set
            {
                this.imageField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public byte position
        {
            get
            {
                return this.positionField;
            }
            set
            {
                this.positionField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmTrackAlbumImage
    {

        private string sizeField;

        private string valueField;

        /// <remarks/>
        [XmlAttribute()]
        public string size
        {
            get
            {
                return this.sizeField;
            }
            set
            {
                this.sizeField = value;
            }
        }

        /// <remarks/>
        [XmlText()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmTrackTag
    {

        private string nameField;

        private string urlField;

        /// <remarks/>
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public string url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmTrackWiki
    {

        private string publishedField;

        private string summaryField;

        private string contentField;

        /// <remarks/>
        public string published
        {
            get
            {
                return this.publishedField;
            }
            set
            {
                this.publishedField = value;
            }
        }

        /// <remarks/>
        public string summary
        {
            get
            {
                return this.summaryField;
            }
            set
            {
                this.summaryField = value;
            }
        }

        /// <remarks/>
        public string content
        {
            get
            {
                return this.contentField;
            }
            set
            {
                this.contentField = value;
            }
        }
    }
}
