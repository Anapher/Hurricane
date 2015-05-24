using System.Xml.Serialization;
// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.Billboard
{
    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public class rssChannelItemGuid
    {
        /// <remarks/>
        [XmlAttribute]
        public bool isPermaLink { get; set; }

        /// <remarks/>
        [XmlText]
        public string Value { get; set; }
    }
}