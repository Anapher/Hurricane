using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Hurricane.Utilities
{
   static class ImageHelper
    {
        public static Size GetMinimumSize(Size size, int maxwidth, int maxheight)
        {
            System.Drawing.Size NewSize;
            if (size.Width == size.Height)
            {
                NewSize = new Size(220, 220);
            }
            else
            {
                bool WidthIsHigher = size.Width > size.Height;
                double ratio = (double)size.Width / (double)size.Height;
                double newwidth;
                double newheight;

                if (WidthIsHigher)
                {
                    newwidth = maxheight * ratio;
                    if (newwidth >= maxwidth)
                    {
                        newheight = maxheight;
                    }
                    else
                    {
                        newwidth = maxwidth;
                        newheight = maxwidth / ratio;
                    }
                }
                else
                {
                    newheight = maxwidth / ratio;
                    if (newheight >= maxheight)
                    {
                        newwidth = maxwidth;
                    }
                    else
                    {
                        newwidth = maxheight * ratio;
                        newheight = maxheight;
                    }
                }
                NewSize = new System.Drawing.Size((int)Math.Round(newwidth, 0), (int)Math.Round(newheight, 0));
            }
            return NewSize;
        }

        public static Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            try
            {
                Bitmap b = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage((Image)b))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
                }
                return b;
            }
            catch { return null; }
        }

        public static Icon GetIconFromResource(string Path)
        {
            return new System.Drawing.Icon(System.Windows.Application.GetResourceStream(new Uri(string.Format("pack://application:,,,/Hurricane;component/{0}", Path))).Stream);
        }

        public static System.Windows.Media.Imaging.BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}
