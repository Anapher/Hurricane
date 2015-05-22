using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Hurricane.Model.Music.TrackProperties
{
    public sealed class Artist : PropertyChangedBase
    {
        private BitmapImage _image;
        private bool _isLoadingImage;

        public string Name { get; set; }
        public string MusicbrainzId { get; set; }
        public string Url { get; set; }
        public List<Artist> SimilarArtists { get; set; }
        public string Summary { get; set; }

        public BitmapImage Image
        {
            get { return _image; }
            set { SetProperty(value, ref _image); }
        }

        public bool IsLoadingImage
        {
            get { return _isLoadingImage; }
            set { SetProperty(value, ref _isLoadingImage); }
        }
    }
}