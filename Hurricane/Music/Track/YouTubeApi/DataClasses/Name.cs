using Newtonsoft.Json;

namespace Hurricane.Music.Track.YouTubeApi.DataClasses
{
    public class Name
    {
        [JsonProperty("$t")]
        public string Text { get; set; }
    }
}
