namespace Hurricane.Services.YouTube.Data.ContentDetailsRequest
{
    class Item
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public ContentDetails contentDetails { get; set; }
    }
}
