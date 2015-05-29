using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Hurricane.Utilities;

namespace Hurricane.Model.Music.TrackProperties
{
    /// <summary>
    /// Provides an image
    /// </summary>
    [Serializable]
    public class ImageProvider : IDisposable, INotifyPropertyChanged
    {
        private static readonly string ImageDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hurricane", "Images");
        private static readonly object LockObject = new object();

        private BitmapImage _image;
        private bool _isLoadingImage;
        private double _downloadProgress;

        public ImageProvider()
        {
            Guid = Guid.NewGuid();
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
            Image?.StreamSource?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The url to the image
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The guid of the image
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Defines if the image should be downloaded to the download folder
        /// </summary>
        [XmlIgnore]
        public bool DownloadImage { get; set; }

        /// <summary>
        /// The image
        /// </summary>
        [XmlIgnore]
        public BitmapImage Image
        {
            get
            {
                return _image;
            }
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
        /// The current download progress (0 - 1)
        /// </summary>
        public double DownloadProgress
        {
            get { return _downloadProgress; }
            set
            {
                if (!value.Equals(_downloadProgress))
                {
                    _downloadProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// If set, the image is currently downloading
        /// </summary>
        [XmlIgnore]
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
            var imageFile = new FileInfo(Path.Combine(ImageDirectory, $"{Guid.ToString("D")}.png"));
            if (imageFile.Exists)
            {
                image.UriSource = new Uri(imageFile.FullName, UriKind.Absolute);
            }
            else
            {
                using (var wc = new WebClient { Proxy = null })
                {
                    wc.DownloadProgressChanged += (sender, args) => DownloadProgress = args.ProgressPercentage/100d;
                    if (DownloadImage)
                    {
                        if(imageFile.Directory?.Exists == false)
                            imageFile.Directory.Create();

                        await wc.DownloadFileTaskAsync(Url, imageFile.FullName);
                        image.UriSource = new Uri(imageFile.FullName, UriKind.Absolute);
                    }
                    else
                    {
                        image.StreamSource = new MemoryStream(await wc.DownloadDataTaskAsync(Url));
                    }
                }
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