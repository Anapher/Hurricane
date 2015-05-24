using System;

namespace Hurricane.Model.Music.TrackProperties
{
    public class TopTrack
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public string MusicbrainzId { get; set; }
        public Artist Artist { get; set; }
        public string Url { get; set; }
        public ImageProvider Thumbnail { get; set; }
    }
}