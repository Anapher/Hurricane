namespace Hurricane.Services.YouTube.Data.ContentDetailsRequest
{
    class ContentDetails
    {
        public string duration { get; set; }
        public string dimension { get; set; }
        public string definition { get; set; }
        public string caption { get; set; }
        public bool licensedContent { get; set; }
        public RegionRestriction regionRestriction { get; set; }
    }
}
