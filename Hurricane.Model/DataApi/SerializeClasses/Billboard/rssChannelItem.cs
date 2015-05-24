using System.Xml.Serialization;

// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.Billboard
{
    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public class rssChannelItem
    {
        /// <remarks/>
        public string title { get; set; }

        /// <remarks/>
        public string link { get; set; }

        /// <remarks/>
        public string artist { get; set; }

        /// <remarks/>
        public string chart_item_title { get; set; }

        /// <remarks/>
        public int rank_this_week { get; set; }

        /// <remarks/>
        public int rank_last_week { get; set; }

        /// <remarks/>
        public string description { get; set; }

        /// <remarks/>
        public string category { get; set; }

        /// <remarks/>
        public string pubDate { get; set; }

        /// <remarks/>
        public rssChannelItemGuid guid { get; set; }
    }
}