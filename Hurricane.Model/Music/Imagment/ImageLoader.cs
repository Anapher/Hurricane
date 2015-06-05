using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hurricane.Utilities;

namespace Hurricane.Model.Music.Imagment
{
    static class ImageLoader
    {
        private static readonly List<ImageProvider> Images;
        private static bool _serviceIsRunning;

        static ImageLoader()
        {
            Images = new List<ImageProvider>();
        }

        public static void AddImage(ImageProvider imageProvider)
        {
            if (Images.Any(x => x.Guid == imageProvider.Guid))
                return;

            Images.Add(imageProvider);
            if (_serviceIsRunning)
                return;

            RunService().Forget();
        }

        private static async Task RunService()
        {
            _serviceIsRunning = true;
            while (Images.Count > 0)
            {
                var currentImage = Images[0];
                await currentImage.LoadImageAsync();
                Images.Remove(currentImage);
            }
            _serviceIsRunning = false;
        }
    }
}
