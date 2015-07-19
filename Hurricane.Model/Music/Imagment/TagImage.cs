using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using File = TagLib.File;

namespace Hurricane.Model.Music.Imagment
{
    public class TagImage : ImageProvider
    {
        /// <summary>
        /// Creates a new instance of <see cref="TagImage"/>
        /// </summary>
        /// <param name="path">The path to the file providing the image</param>
        public TagImage(string path)
        {
            FilePath = path;
        }

        /// <summary>
        /// For XML-Serialization
        /// </summary>
        private TagImage()
        {
            
        }

        /// <summary>
        /// The path to the file
        /// </summary>
        [XmlAttribute]
        public string FilePath { get; set; }

        protected override Task<BitmapImage> LoadImage()
        {
            throw new NotImplementedException();
        }

        protected override async Task<BitmapImage> GetImageFast()
        {
            using (var tagFile = File.Create(FilePath))
            {
                BitmapImage bitmapImage = null;
                var ms = new MemoryStream(tagFile.Tag.Pictures.First().Data.Data);
                try
                {
                    await Task.Run(() =>
                    {
                        bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                    });
                }
                catch (NotSupportedException)
                {
                    //Fuck it
                    ms.Dispose();
                    return null;
                }

                return bitmapImage;
            }
        }
    }
}