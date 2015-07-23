using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Hurricane.Model.Music.Imagment
{
    /// <summary>
    /// Provides an image
    /// </summary>
    [Serializable, XmlInclude(typeof(OnlineImage)), XmlInclude(typeof(TagImage))]
    public abstract class ImageProvider : IDisposable, INotifyPropertyChanged
    {
        protected static readonly string ImageDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hurricane", "Images");

        private BitmapImage _image;
        private bool _isLoadingImage;
        private double _loadProgress;

        protected ImageProvider()
        {
            Guid = Guid.NewGuid();
        }

        public void Dispose()
        {
            Image?.StreamSource?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The guid of the image
        /// </summary>
        [XmlAttribute]
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
                if (_image == null)
                    BeginLoadingImage();
                
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
        /// The current loading progress (0 - 1)
        /// </summary>
        [XmlIgnore]
        public double LoadProgress
        {
            get { return _loadProgress; }
            set
            {
                if (!value.Equals(_loadProgress))
                {
                    _loadProgress = value;
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
        /// Loads the image
        /// </summary>
        /// <returns></returns>
        public async Task LoadImageAsync()
        {
            if (Image != null) return;
            Image = await LoadImage();
            IsLoadingImage = false;
        }

        public async void BeginLoadingImage(bool highPriority = false)
        {
            IsLoadingImage = true;
            var image = await GetImageFast();
            if (image == null)
            {
                ImageLoader.AddImage(this);
                return;
            }

            Image = image;
            IsLoadingImage = false;
        }

        public bool ShouldSerializeGuid()
        {
            return Guid != Guid.Empty;
        }

        protected abstract Task<BitmapImage> LoadImage();
        protected abstract Task<BitmapImage> GetImageFast();

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}