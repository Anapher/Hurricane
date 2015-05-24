using System.Xml.Schema;
using System.Xml.Serialization;
// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.Billboard
{
    /// <remarks/>
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class rss
    {
        public rssChannel channel { get; set; }

        [XmlAttribute]
        public decimal version { get; set; }

        [XmlAttribute(Form = XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string @base { get; set; }
    }
}