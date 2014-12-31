using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using Size = System.Drawing.Size;

namespace Hurricane.Utilities
{
   static class ImageHelper
    {
        public static Size GetMinimumSize(Size size, int maxwidth, int maxheight)
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
                newSize = new Size((int)Math.Round(newwidth, 0), (int)Math.Round(newheight, 0));
            }
            return newSize;
        }

        public static Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            try
            {
                Bitmap b = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
                }
                return b;
            }
            catch { return null; }
        }

        public static Icon GetIconFromResource(string path)
        {
            return new Icon(Application.GetResourceStream(new Uri(string.Format("pack://application:,,,/Hurricane;component/{0}", path))).Stream);
        }
    }
}
