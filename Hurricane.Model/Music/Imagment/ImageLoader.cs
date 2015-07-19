using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hurricane.Utilities;

namespace Hurricane.Model.Music.Imagment
{
    static class ImageLoader
    {
        private static readonly List<ImageProvider> Images;
        private static readonly List<ImageProvider> ImportantImages; 
        private static bool _serviceIsRunning;

        static ImageLoader()
        {
            Images = new List<ImageProvider>();
            ImportantImages = new List<ImageProvider>();
        }

        public static void AddImage(ImageProvider imageProvider, bool highPriorityQueue = false)
        {
            if (Images.Any(x => x.Guid == imageProvider.Guid))
            {
                if (ImportantImages.All(x => x.Guid != imageProvider.Guid) && highPriorityQueue)
                {
                    Images.Remove(imageProvider); //Level up
                    ImportantImages.Add(imageProvider);
                }
                return;
            }

            if (ImportantImages.Any(x => x.Guid == imageProvider.Guid))
                return;

            if (highPriorityQueue)
                ImportantImages.Add(imageProvider);
            else Images.Add(imageProvider);

            if (_serviceIsRunning)
                return;

            RunService().Forget();
        }

        private static async Task RunService()
        {
            _serviceIsRunning = true;
            while (Images.Count > 0 || ImportantImages.Count > 0)
            {
                var currentList = ImportantImages.Count > 0 ? ImportantImages : Images;
                var currentImage = currentList[0];

                await currentImage.LoadImageAsync();
                currentList.Remove(currentImage);
                currentImage.IsLoadingImage = false;
            }
            _serviceIsRunning = false;
        }
    }
}
