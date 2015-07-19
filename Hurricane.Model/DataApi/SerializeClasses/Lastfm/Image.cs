using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.Lastfm
{
    class Image
    {
        [JsonProperty("#text")]
        public string text { get; set; }
        public ImageSize size { get; set; }
    }

    enum ImageSize
    {
        small,
        medium,
        large,
        extralarge,
        mega
    }
}