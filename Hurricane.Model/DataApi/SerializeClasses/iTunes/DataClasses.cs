using System.Collections.Generic;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Hurricane.Model.DataApi.SerializeClasses.iTunes
{
    class Name
    {
        public string label { get; set; }
    }

    class Uri
    {
        public string label { get; set; }
    }

    class Author
    {
        public Name name { get; set; }
        public Uri uri { get; set; }
    }

    class ImName
    {
        public string label { get; set; }
    }

    class Attributes
    {
        public string height { get; set; }
    }

    class ImImage
    {
        public string label { get; set; }
        public Attributes attributes { get; set; }
    }

    class ImName2
    {
        public string label { get; set; }
    }

    class Attributes2
    {
        public string rel { get; set; }
        public string type { get; set; }
        public string href { get; set; }
    }

    class Link
    {
        public Attributes2 attributes { get; set; }
    }

    class Attributes3
    {
        public string term { get; set; }
        public string label { get; set; }
    }

    class ImContentType2
    {
        public Attributes3 attributes { get; set; }
    }

    class Attributes4
    {
        public string term { get; set; }
        public string label { get; set; }
    }

    class ImContentType
    {
        [JsonProperty("im:contentType")]
        public ImContentType2 ContentType { get; set; }
        public Attributes4 attributes { get; set; }
    }

    class ImCollection
    {
        [JsonProperty("im:name")]
        public ImName2 Name { get; set; }
        public Link link { get; set; }
        [JsonProperty("im:contentType")]
        public ImContentType ContentType { get; set; }
    }

    class Attributes5
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }

    class ImPrice
    {
        public string label { get; set; }
        public Attributes5 attributes { get; set; }
    }

    class Attributes6
    {
        public string term { get; set; }
        public string label { get; set; }
    }

    class ImContentType4
    {
        public Attributes6 attributes { get; set; }
    }

    class Attributes7
    {
        public string term { get; set; }
        public string label { get; set; }
    }

    class ImContentType3
    {
        [JsonProperty("im:contentType")]
        public ImContentType4 ContentType { get; set; }
        public Attributes7 attributes { get; set; }
    }

    class Rights
    {
        public string label { get; set; }
    }

    class Title
    {
        public string label { get; set; }
    }

    class Attributes8
    {
        public string rel { get; set; }
        public string type { get; set; }
        public string href { get; set; }
        public string title { get; set; }
        [JsonProperty("im:assetType")]
        public string AssetType { get; set; }
    }

    class ImDuration
    {
        public string label { get; set; }
    }

    class Link2
    {
        public Attributes8 attributes { get; set; }
        [JsonProperty("im:duration")]
        public ImDuration Duration { get; set; }
    }

    class Attributes9
    {
        [JsonProperty("im:id")]
        public string Id { get; set; }
    }

    class Id
    {
        public string label { get; set; }
        public Attributes9 attributes { get; set; }
    }

    class Attributes10
    {
        public string href { get; set; }
    }

    class ImArtist
    {
        public string label { get; set; }
        public Attributes10 attributes { get; set; }
    }

    class Attributes11
    {
        [JsonProperty("im:id")]
        public string id { get; set; }
        public string term { get; set; }
        public string scheme { get; set; }
        public string label { get; set; }
    }

    class Category
    {
        public Attributes11 attributes { get; set; }
    }

    class Attributes12
    {
        public string label { get; set; }
    }

    class ImReleaseDate
    {
        public string label { get; set; }
        public Attributes12 attributes { get; set; }
    }

    class Entry
    {
        [JsonProperty("im:name")]
        public ImName Name { get; set; }
        [JsonProperty("im:image")]
        public List<ImImage> Image { get; set; }
        [JsonProperty("im:collection")]
        public ImCollection Collection { get; set; }
        [JsonProperty("im:price")]
        public ImPrice Price { get; set; }
        [JsonProperty("im:contentType")]
        public ImContentType3 ContentType { get; set; }
        public Rights rights { get; set; }
        public Title title { get; set; }
        public List<Link2> link { get; set; }
        public Id id { get; set; }
        [JsonProperty("im:artist")]
        public ImArtist Artist { get; set; }
        public Category category { get; set; }
        [JsonProperty("im:releaseDate")]
        public ImReleaseDate ReleaseDate { get; set; }
    }

    class Updated
    {
        public string label { get; set; }
    }

    class Rights2
    {
        public string label { get; set; }
    }

    class Title2
    {
        public string label { get; set; }
    }

    class Icon
    {
        public string label { get; set; }
    }

    class Attributes13
    {
        public string rel { get; set; }
        public string type { get; set; }
        public string href { get; set; }
    }

    class Link3
    {
        public Attributes13 attributes { get; set; }
    }

    class Id2
    {
        public string label { get; set; }
    }

    class Feed
    {
        public Author author { get; set; }
        public List<Entry> entry { get; set; }
        public Updated updated { get; set; }
        public Rights2 rights { get; set; }
        public Title2 title { get; set; }
        public Icon icon { get; set; }
        public List<Link3> link { get; set; }
        public Id2 id { get; set; }
    }

    class RssFeed
    {
        public Feed feed { get; set; }
    }
}