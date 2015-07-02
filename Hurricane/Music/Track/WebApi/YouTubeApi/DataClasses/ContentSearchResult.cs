using System.Collections.Generic;
using Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses.SearchResult;

namespace Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses
{
    public class RegionRestriction
    {
        public List<string> blocked { get; set; }
    }

    public class ContentDetails
    {
        public string duration { get; set; }
        public string dimension { get; set; }
        public string definition { get; set; }
        public string caption { get; set; }
        public bool licensedContent { get; set; }
        public RegionRestriction regionRestriction { get; set; }
    }

    
    public class Item2
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public ContentDetails contentDetails { get; set; }
    }

    public class ContentSearchResult
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public PageInfo pageInfo { get; set; }
        public List<Item2> items { get; set; }
    }
}
