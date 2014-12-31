using System.Xml.Serialization;

namespace Hurricane.Music.MusicDatabase
{
    public partial class lfm
    {

        private lfmResults resultsField;

        /// <remarks/>
        public lfmResults results
        {
            get
            {
                return this.resultsField;
            }
            set
            {
                this.resultsField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmResults
    {

        private Query queryField;

        private ushort totalResultsField;

        private byte startIndexField;

        private byte itemsPerPageField;

        private lfmResultsTrack[] trackmatchesField;

        private string forField;

        /// <remarks/>
        [XmlElement(Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public Query Query
        {
            get
            {
                return this.queryField;
            }
            set
            {
                this.queryField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public ushort totalResults
        {
            get
            {
                return this.totalResultsField;
            }
            set
            {
                this.totalResultsField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public byte startIndex
        {
            get
            {
                return this.startIndexField;
            }
            set
            {
                this.startIndexField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public byte itemsPerPage
        {
            get
            {
                return this.itemsPerPageField;
            }
            set
            {
                this.itemsPerPageField = value;
            }
        }

        /// <remarks/>
        [XmlArrayItem("track", IsNullable = false)]
        public lfmResultsTrack[] trackmatches
        {
            get
            {
                return this.trackmatchesField;
            }
            set
            {
                this.trackmatchesField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string @for
        {
            get
            {
                return this.forField;
            }
            set
            {
                this.forField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
    [XmlRoot(Namespace = "http://a9.com/-/spec/opensearch/1.1/", IsNullable = false)]
    public partial class Query
    {

        private string roleField;

        private string searchTermsField;

        private byte startPageField;

        /// <remarks/>
        [XmlAttribute()]
        public string role
        {
            get
            {
                return this.roleField;
            }
            set
            {
                this.roleField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string searchTerms
        {
            get
            {
                return this.searchTermsField;
            }
            set
            {
                this.searchTermsField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public byte startPage
        {
            get
            {
                return this.startPageField;
            }
            set
            {
                this.startPageField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmResultsTrack
    {

        private string nameField;

        private string artistField;

        private string urlField;

        private lfmResultsTrackStreamable streamableField;

        private lfmResultsTrackImage[] imageField;

        private string mbidField;

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
        public lfmResultsTrackStreamable streamable
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
        [XmlElement("image")]
        public lfmResultsTrackImage[] image
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
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class lfmResultsTrackStreamable
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
    public partial class lfmResultsTrackImage
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


}
