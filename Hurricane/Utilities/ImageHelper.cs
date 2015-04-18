using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Size = System.Drawing.Size;

namespace Hurricane.Utilities
{
    public static class ImageHelper
    {
        /// <summary>
        /// Calculates the minimum size for the picture in compliance to the proportion: <see cref="size"/> = 900x500, <see cref="maxWidth"/> = 400, <see cref="maxHeight"/> = 200 returns 400x222
        /// </summary>
        /// <param name="size">The current size of the image</param>
        /// <param name="maxWidth">The new maximum width</param>
        /// <param name="maxHeight">The new minimum height</param>
        /// <returns>The new width</returns>
        public static Size GetMinimumSize(Size size, int maxWidth, int maxHeight)
        {
            Size newSize;
            if (size.Width == size.Height)
            {
                newSize = new Size(220, 220);
            }
            else
            {
                bool widthIsHigher = size.Width > size.Height;
                double ratio = size.Width / (double)size.Height;
                double newwidth;
                double newheight;

                if (widthIsHigher)
                {
                    newwidth = maxHeight * ratio;
                    if (newwidth >= maxWidth)
                    {
                        newheight = maxHeight;
                    }
                    else
                    {
                        newwidth = maxWidth;
                        newheight = maxWidth / ratio;
                    }
                }
                else
                {
                    newheight = maxWidth / ratio;
                    if (newheight >= maxHeight)
                    {
                        newwidth = maxWidth;
                    }
                    else
                    {
                        newwidth = maxHeight * ratio;
                        newheight = maxHeight;
                    }
                }
                newSize = new Size((int)Math.Round(newwidth, 0), (int)Math.Round(newheight, 0));
            }
            return newSize;
        }

        /// <summary>
        /// Resizes an image
        /// </summary>
        /// <param name="image">The image which should be resized</param>
        /// <param name="size">The new size of the image</param>
        /// <returns>The resized image</returns>
        public static Bitmap ResizeImage(Bitmap image, Size size)
        {
            try
            {
                Bitmap b = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(image, 0, 0, size.Width, size.Height);
                }
                return b;
            }
            catch { return null; }
        }

        /// <summary>
        /// Returns the resource as icon
        /// </summary>
        /// <param name="path">The resource path</param>
        /// <returns>The icon</returns>
        public static Icon GetIconFromResource(string path)
        {
            var streamResourceInfo = Application.GetResourceStream(new Uri(string.Format("pack://application:,,,/Hurricane;component/{0}", path)));
            if (streamResourceInfo == null)
                throw new ArgumentException(path);
            return new Icon(streamResourceInfo.Stream);
        }

        /// <summary>
        /// Downloads the <see cref="BitmapImage"/> from the give <see cref="url"/> with the <see cref="webClient"/>
        /// </summary>
        /// <param name="webClient">The web client</param>
        /// <param name="url">The url to the image</param>
        /// <returns>The image</returns>
        public async static Task<BitmapImage> DownloadImage(WebClient webClient, string url)
        {
            using (var mr = new MemoryStream(await webClient.DownloadDataTaskAsync(url)))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = mr;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }

        /// <summary>
        /// Saves the image as a png file
        /// </summary>
        /// <param name="image">The <see cref="BitmapImage"/> which should be saved</param>
        /// <param name="fileName">The file name without an extension</param>
        /// <param name="directory">The dirctory of the image</param>
        /// <returns></returns>
        public static async Task SaveImage(BitmapImage image, string fileName, string directory)
        {
            await Task.Run(() =>
            {
                var encoder = new PngBitmapEncoder();
                string path = Path.Combine(directory, fileName.ToEscapedFilename() + ".png");
                encoder.Frames.Add(BitmapFrame.Create(image));
                using (var filestream = new FileStream(path, FileMode.Create))
                    encoder.Save(filestream);
            });
        }

        /// <summary>
        /// Converts the byte array to a <see cref="BitmapImage"/>
        /// </summary>
        /// <param name="data">The byte array which contains an image</param>
        /// <returns>The converted image</returns>
        public static BitmapImage ByteArrayToBitmapImage(byte[] data)
        {
            return StreamToBitmapImage(new MemoryStream(data));
        }

        /// <summary>
        /// Reads a <see cref="BitmapImage"/> from the stream
        /// </summary>
        /// <param name="stream">The stream which contains the image</param>
        /// <returns>The image from the stream</returns>
        public static BitmapImage StreamToBitmapImage(Stream stream)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }
    }
}