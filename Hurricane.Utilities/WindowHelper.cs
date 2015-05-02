using System;

namespace Hurricane.Utilities
{
    public class WindowHelper
    {
        // ReSharper disable InconsistentNaming
        private const int GWL_STYLE = -16;
        private const uint WS_MAXIMIZEBOX = 0x10000;
        private const uint WS_MINIMIZEBOX = 0x20000;
        private const uint WS_OVERLAPPED = 0x00000000;
        private const uint WS_CAPTION = 0x00C00000;
        private const uint WS_SYSMENU = 0x00080000;
        private const uint WS_THICKFRAME = 0x00040000;
        private const uint WS_OVERLAPPEDWINDOW =
            (WS_OVERLAPPED |
             WS_CAPTION |
             WS_SYSMENU |
             WS_THICKFRAME |
             WS_MINIMIZEBOX |
             WS_MAXIMIZEBOX);

        public static void DisableAeroSnap(IntPtr handle)
        {
            var currentStyle = UnsafeNativeMethods.GetWindowLong(handle, GWL_STYLE);
            currentStyle |= (int)WS_OVERLAPPEDWINDOW;
            currentStyle ^= (int)WS_THICKFRAME;
            UnsafeNativeMethods.SetWindowLong(handle, GWL_STYLE, currentStyle);
        }
    }
}