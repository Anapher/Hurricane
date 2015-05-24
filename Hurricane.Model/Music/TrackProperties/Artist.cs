using System.Collections.Generic;

namespace Hurricane.Model.Music.TrackProperties
{
    /// <summary>
    /// Provides information about an artist
    /// </summary>
    public class Artist : PropertyChangedBase
    {
        private string _biography;
        private List<Artist> _similarArtists;
        private bool _providesAdvancedInfo;
        private List<TopTrack> _topTracks;

        /// <summary>
        /// The name of the artist
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The <see href="https://musicbrainz.org/doc/MusicBrainz_Identifier">MusicBrainz Identifier</see>
        /// </summary>
        public string MusicbrainzId { get; set; }

        /// <summary>
        /// The <see href="http://wwww.last.fm/">Last.fm</see> url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Similar artists
        /// </summary>
        public List<Artist> SimilarArtists
        {
            get { return _similarArtists; }
            set { SetProperty(value, ref _similarArtists); }
        }

        /// <summary>
        /// A short biography (max. 300 chars)
        /// </summary>
        public string Biography
        {
            get { return _biography; }
            set { SetProperty(value, ref _biography); }
        }

        /// <summary>
        /// Returns if the properties <see cref="Biography"/>, <see cref="TopTracks"/> and <see cref="SimilarArtists"/> are set
        /// </summary>
        public bool ProvidesAdvancedInfo
        {
            get { return _providesAdvancedInfo; }
            set { SetProperty(value, ref _providesAdvancedInfo); }
        }

        /// <summary>
        /// Top tracks of the artist
        /// </summary>
        public List<TopTrack> TopTracks
        {
            get { return _topTracks; }
            set { SetProperty(value, ref _topTracks); }
        }

        /// <summary>
        /// The small sized image of the artist (34x34px)
        /// </summary>
        public ImageProvider SmallImage { get; set; }

        /// <summary>
        /// The medium sized image of the artist (~64x64px)
        /// </summary>
        public ImageProvider MediumImage { get; set; }

        /// <summary>
        /// The really large image of the artist (~500x500px)
        /// </summary>
        public ImageProvider LargeImage { get; set; }
    }
}