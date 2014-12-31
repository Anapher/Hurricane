using System.Xml.Serialization;

namespace Hurricane.Music.MusicDatabase
{

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public partial class lfm
    {

        private lfmArtist artistField;

        private string statusField;

        /// <remarks/>
        public lfmArtist artist
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
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmArtist
    {

        private string nameField;

        private string mbidField;

        private string urlField;

        private lfmArtistImage[] imageField;

        private byte streamableField;

        private byte ontourField;

        private lfmArtistStats statsField;

        private object similarField;

        private lfmArtistTag[] tagsField;

        private lfmArtistBio bioField;

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

        /// <remarks/>
        [XmlElement("image")]
        public lfmArtistImage[] image
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
        public byte streamable
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
        public byte ontour
        {
            get
            {
                return this.ontourField;
            }
            set
            {
                this.ontourField = value;
            }
        }

        /// <remarks/>
        public lfmArtistStats stats
        {
            get
            {
                return this.statsField;
            }
            set
            {
                this.statsField = value;
            }
        }

        /// <remarks/>
        public object similar
        {
            get
            {
                return this.similarField;
            }
            set
            {
                this.similarField = value;
            }
        }

        /// <remarks/>
        [XmlArrayItem("tag", IsNullable = false)]
        public lfmArtistTag[] tags
        {
            get
            {
                return this.tagsField;
            }
            set
            {
                this.tagsField = value;
            }
        }

        /// <remarks/>
        public lfmArtistBio bio
        {
            get
            {
                return this.bioField;
            }
            set
            {
                this.bioField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmArtistImage
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
    public partial class lfmArtistStats
    {

        private uint listenersField;

        private uint playcountField;

        /// <remarks/>
        public uint listeners
        {
            get
            {
                return this.listenersField;
            }
            set
            {
                this.listenersField = value;
            }
        }

        /// <remarks/>
        public uint playcount
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
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmArtistTag
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
    public partial class lfmArtistBio
    {

        private lfmArtistBioLinks linksField;

        private string publishedField;

        private string summaryField;

        private string contentField;

        /// <remarks/>
        public lfmArtistBioLinks links
        {
            get
            {
                return this.linksField;
            }
            set
            {
                this.linksField = value;
            }
        }

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

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmArtistBioLinks
    {

        private lfmArtistBioLinksLink linkField;

        /// <remarks/>
        public lfmArtistBioLinksLink link
        {
            get
            {
                return this.linkField;
            }
            set
            {
                this.linkField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmArtistBioLinksLink
    {

        private string relField;

        private string hrefField;

        /// <remarks/>
        [XmlAttribute()]
        public string rel
        {
            get
            {
                return this.relField;
            }
            set
            {
                this.relField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string href
        {
            get
            {
                return this.hrefField;
            }
            set
            {
                this.hrefField = value;
            }
        }
    }


}
