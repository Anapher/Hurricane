using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Hurricane.Model.Music.Imagment
{
    [Serializable]
    public class OnlineImage : ImageProvider
    {
        /// <summary>
        /// Creates a new instance of <see cref="OnlineImage"/>
        /// </summary>
        /// <param name="url">The url to the image</param>
        public OnlineImage(string url)
        {
            Url = url;
        }

        /// <summary>
        /// For XML-Serialization
        /// </summary>
        private OnlineImage()
        {
            
        }

        /// <summary>
        /// The url to the image
        /// </summary>
        [XmlAttribute]
        public string Url { get; set; }

        protected async override Task<BitmapImage> LoadImage()
        {
            var image = new BitmapImage();
            image.BeginInit();
            var imageFile = new FileInfo(Path.Combine(ImageDirectory, $"{Guid.ToString("D")}.png"));
            using (var wc = new WebClient { Proxy = null })
            {
                Debug.Print($"Downloading image {Url}");
                wc.DownloadProgressChanged += (sender, args) => LoadProgress = args.ProgressPercentage / 100d;
                if (DownloadImage)
                {
                    if (imageFile.Directory?.Exists == false)
                        imageFile.Directory.Create();

                    await wc.DownloadFileTaskAsync(Url, imageFile.FullName);
                    image.UriSource = new Uri(imageFile.FullName, UriKind.Absolute);
                }
                else
                {
                    image.StreamSource = new MemoryStream(await wc.DownloadDataTaskAsync(Url));
                }
            }
            try
            {
                image.EndInit();
            }
            catch (NotSupportedException)
            {
                image.StreamSource?.Dispose();
                return null;
            }
            
            image.Freeze();
            return image;
        }

        protected async override Task<BitmapImage> GetImageFast()
        {
            var imageFile = new FileInfo(Path.Combine(ImageDirectory, $"{Guid.ToString("D")}.png"));
            if (!imageFile.Exists)
                return null;

            BitmapImage bitmapImage = null;
            await Task.Run(() =>
            {
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(imageFile.FullName);
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            });

            return bitmapImage;
        }
    }
}