using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Hurricane.Model.Music.TrackProperties
{
    /// <summary>
    /// Provides an image
    /// </summary>
    public class ImageProvider : IDisposable, INotifyPropertyChanged
    {
        private BitmapImage _image;
        private bool _isLoadingImage;

        public ImageProvider()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ImageProvider"/>
        /// </summary>
        /// <param name="url">The url to the image</param>
        public ImageProvider(string url)
        {
            Url = url;
        }

        public void Dispose()
        {
            Image?.StreamSource.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The url to the image
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The image
        /// </summary>
        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                if (!Equals(value, _image))
                {
                    _image = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// If set, the image is currently downloading
        /// </summary>
        public bool IsLoadingImage
        {
            get { return _isLoadingImage; }
            set
            {
                if (value != _isLoadingImage)
                {
                    _isLoadingImage = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Download the image
        /// </summary>
        /// <returns></returns>
        public async Task LoadImageAsync()
        {
            if (IsLoadingImage || Image != null) return;
            IsLoadingImage = true;
            var image = new BitmapImage();
            image.BeginInit();
            using (var wc = new WebClient { Proxy = null })
            {
                image.StreamSource = new MemoryStream(await wc.DownloadDataTaskAsync(Url));
            }
            image.EndInit();
            Image = image;
            IsLoadingImage = false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}