using System.Xml.Serialization;

// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.Billboard
{
    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public class rssChannel
    {
        /// <remarks/>
        public string title { get; set; }

        /// <remarks/>
        public string link { get; set; }

        /// <remarks/>
        public string description { get; set; }

        /// <remarks/>
        public string language { get; set; }

        /// <remarks/>
        [XmlElement("item")]
        public rssChannelItem[] item { get; set; }
    }
}