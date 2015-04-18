using Newtonsoft.Json;

namespace Hurricane.Music.Track.WebApi.VkontakteApi
{
    [JsonObject(Title = "Response")]
    public class Audio
    {
        public int aid { get; set; }
        public int owner_id { get; set; }
        public string artist { get; set; }
        public string title { get; set; }
        public int duration { get; set; }
        public string url { get; set; }
        public string lyrics_id { get; set; }
        public string album { get; set; }
        public AudioGenre genre { get; set; }
    }

    public enum AudioGenre
    {
        Rock = 1,
        Pop = 2,
        RapAndHipHop = 3,
        EasyListening = 4,
        DanceAndHouse = 5,
        Instrumental = 6,
        Metal = 7,
        Alternative = 21,
        Dubstep = 8,
        JazzAndBlues = 9,
        DrumAndBass = 10,
        Trance = 11,
        Chanson = 12,
        Ethnic = 13,
        AcousticAndVocal = 14,
        Reggae = 15,
        Classical = 16,
        IndiePop = 17,
        Speech = 19,
        ElectropopAndDisco = 22,
        Other = 18
    }
}
