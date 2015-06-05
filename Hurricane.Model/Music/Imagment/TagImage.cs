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

        protected override async Task<BitmapImage> LoadImage()
        {
            using (var tagFile = File.Create(FilePath))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                await
                    Task.Run(() => bitmapImage.StreamSource = new MemoryStream(tagFile.Tag.Pictures.First().Data.Data));
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        protected override bool GetImageFast(out BitmapImage image)
        {
            image = null;
            return false;
        }
    }
}