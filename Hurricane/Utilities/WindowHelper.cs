using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Hurricane.Utilities.Native;

namespace Hurricane.Utilities
{
    class WindowHelper
    {
        public static string GetActiveWindowTitle(IntPtr handle)
        {
            const int nChars = 256;
            StringBuilder buffer = new StringBuilder(nChars);

            if (UnsafeNativeMethods.GetWindowText(handle, buffer, nChars) > 0)
            {
                return buffer.ToString();
            }
            return null;
        }

        public static bool WindowIsFullscreen(IntPtr window)
        {
            var placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            UnsafeNativeMethods.GetWindowPlacement(window, ref placement);
            var workarea = SystemParameters.WorkArea;
            string cname = GetClassName(window);
            return ((placement.showCmd == 1 && placement.minPosition.X == -1 && placement.minPosition.Y == -1 && placement.normalPosition.left == 0 && placement.normalPosition.top == 0 && placement.normalPosition.Width == workarea.Width && !(cname == "Progman" || cname == "WorkerW")));
        }

        public static RECT GetWindowRectangle(Window window)
        {
            RECT rect;
            UnsafeNativeMethods.GetWindowRect((new WindowInteropHelper(window)).Handle, out rect);
            return rect;
        }

        public static string GetClassName(IntPtr handle)
        {
            const int maxChars = 256;
            StringBuilder className = new StringBuilder(maxChars);
            if (UnsafeNativeMethods.GetClassName(handle, className, maxChars) > 0)
            {
                return className.ToString();
            }
            return string.Empty;
        }

        private const int GWL_STYLE = -16,
                          WS_MAXIMIZEBOX = 0x10000,
                          WS_MINIMIZEBOX = 0x20000;



        internal static void HideMinimizeAndMaximizeButtons(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            var currentStyle = UnsafeNativeMethods.GetWindowLong(hwnd, GWL_STYLE);

            UnsafeNativeMethods.SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
        }
    }
}
