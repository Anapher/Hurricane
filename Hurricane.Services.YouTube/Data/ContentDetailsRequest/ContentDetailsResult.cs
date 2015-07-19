using System.Collections.Generic;

namespace Hurricane.Services.YouTube.Data.ContentDetailsRequest
{
    class ContentDetailsResult
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public PageInfo pageInfo { get; set; }
        public List<Item> items { get; set; }
    }
}
