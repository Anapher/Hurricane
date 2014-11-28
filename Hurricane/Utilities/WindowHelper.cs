using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Hurricane.Utilities
{
    class WindowHelper
    {
        /// <summary>The GetForegroundWindow function returns a handle to the foreground window.</summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        private static string GetActiveWindowTitle(IntPtr handle)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        public static bool WindowIsFullscreen(IntPtr window)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(window, ref placement);
            var workarea = System.Windows.SystemParameters.WorkArea;
            return ((placement.showCmd == 1 && placement.ptMinPosition.X == -1 && placement.ptMinPosition.Y == -1 && placement.rcNormalPosition.X == 0 && placement.rcNormalPosition.Y == 0 && placement.rcNormalPosition.Width == workarea.Width && !(GetActiveWindowTitle(window) == "Program Manager")));
            /*
            System.Diagnostics.Debug.Print("==================================");
            System.Diagnostics.Debug.Print(GetActiveWindowTitle(ForegroundWindow));
            System.Diagnostics.Debug.Print("showCmd: {0}", placement.showCmd);
            System.Diagnostics.Debug.Print("rcNormalPosition: {0}", placement.rcNormalPosition);
            System.Diagnostics.Debug.Print("ptMinPosition: {0}", placement.ptMinPosition);
            System.Diagnostics.Debug.Print("ptMaxPosition: {0}", placement.ptMaxPosition);
            System.Diagnostics.Debug.Print("==================================");
             * */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        public static RECT GetWindowRectangle(Window window)
        {
            RECT rect;
            GetWindowRect((new WindowInteropHelper(window)).Handle, out rect);
            return rect;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}
