using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Point = System.Windows.Point;

namespace Hurricane.Utilities
{
    public class WpfScreen
    {
        public static IEnumerable<WpfScreen> AllScreens()
        {
            return Screen.AllScreens.Select(screen => new WpfScreen(screen));
        }

        public static WpfScreen GetScreenFrom(Window window)
        {
            WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
            Screen screen = Screen.FromHandle(windowInteropHelper.Handle);
            WpfScreen wpfScreen = new WpfScreen(screen);
            return wpfScreen;
        }

        public static WpfScreen GetScreenFrom(Point point)
        {
            int x = (int)Math.Round(point.X);
            int y = (int)Math.Round(point.Y);

            // are x,y device-independent-pixels ??
            System.Drawing.Point drawingPoint = new System.Drawing.Point(x, y);
            Screen screen = Screen.FromPoint(drawingPoint);
            WpfScreen wpfScreen = new WpfScreen(screen);

            return wpfScreen;
        }

        public static double MaxHeight
        {
            get
            {

                double i = 0;
                foreach (var s in Screen.AllScreens)
                    if (s.Bounds.Height > i) i = s.Bounds.Height;
                return i;
            }
        }

        protected static double allscreenswidth = -1;
        public static double AllScreensWidth
        {
            get
            {
                if (allscreenswidth == -1)
                {
                    allscreenswidth = 0;
                    foreach (var screen in AllScreens())
                        allscreenswidth += screen.WorkingArea.Width;
                }
                return allscreenswidth;
            }
        }

        public static WpfScreen Primary
        {
            get { return new WpfScreen(Screen.PrimaryScreen); }
        }

        private readonly Screen _screen;

        internal WpfScreen(Screen screen)
        {
            this._screen = screen;
        }

        public Rect DeviceBounds
        {
            get { return this.GetRect(this._screen.Bounds); }
        }

        public Rect WorkingArea
        {
            get { return this.GetRect(this._screen.WorkingArea); }
        }

        private Rect GetRect(Rectangle value)
        {
            return new Rect
            {
                X = value.X,
                Y = value.Y,
                Width = value.Width,
                Height = value.Height
            };
        }

        public bool IsPrimary
        {
            get { return this._screen.Primary; }
        }

        public string DeviceName
        {
            get { return this._screen.DeviceName; }
        }
    }
}
