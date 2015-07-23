using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Hurricane.Model.Music.Imagment
{
    public class BitmapImageProvider : ImageProvider
    {
        private readonly BitmapImage _image;

        public BitmapImageProvider(BitmapImage image)
        {
            _image = image;
        }

        protected override Task<BitmapImage> LoadImage()
        {
            return Task.FromResult(_image);
        }

        protected override Task<BitmapImage> GetImageFast()
        {
            return Task.FromResult(_image);
        }
    }
}