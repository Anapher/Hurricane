using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
            StringBuilder Buff = new StringBuilder(nChars);

            if (UnsafeNativeMethods.GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        public static bool WindowIsFullscreen(IntPtr window)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            UnsafeNativeMethods.GetWindowPlacement(window, ref placement);
            var workarea = System.Windows.SystemParameters.WorkArea;
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
    }
}
