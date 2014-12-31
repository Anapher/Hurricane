using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Hurricane.Utilities.Native
{
    /// <summary>
    /// Contains message information from a thread's message queue.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Message
    {
        public IntPtr hWnd;
        public uint msg;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public Point p;
    }
}
