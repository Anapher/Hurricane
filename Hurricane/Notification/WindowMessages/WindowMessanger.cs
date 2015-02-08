using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Hurricane.Utilities.Native;

namespace Hurricane.Notification.WindowMessages
{
    class WindowMessanger
    {
        public const int WM_OPENMUSICFILE = 0x4A;
        public const int WM_BRINGTOFRONT = 3532;

        public event EventHandler BringWindowToFront;
        public event EventHandler<PlayTrackEventArgs> PlayMusicFile;

        public WindowMessanger(Window baseWindow)
        {
            baseWindow.SourceInitialized += (s,e) =>
            {
                HwndSource source = PresentationSource.FromVisual(baseWindow) as HwndSource;
                source.AddHook(WndProc);
            };
        }
        
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_BRINGTOFRONT:
                    if (BringWindowToFront != null) BringWindowToFront(this, EventArgs.Empty);
                    break;
                case WM_OPENMUSICFILE:
                    if (PlayMusicFile != null)
                    {
                        CopyDataStruct st = (CopyDataStruct)Marshal.PtrToStructure(lParam, typeof(CopyDataStruct));
                        string strData = Marshal.PtrToStringUni(st.lpData);
                        PlayMusicFile(this, new PlayTrackEventArgs(strData));
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        public static void SendMessageToWindow(IntPtr hwnd, int message, string parameter)
        {
            CopyDataStruct cds = new CopyDataStruct();
            try
            {
                cds.cbData = (parameter.Length + 1) * 2;
                cds.lpData = UnsafeNativeMethods.LocalAlloc(0x40, cds.cbData);
                Marshal.Copy(parameter.ToCharArray(), 0, cds.lpData, parameter.Length);
                cds.dwData = (IntPtr)1;
                UnsafeNativeMethods.SendMessage(hwnd, message, IntPtr.Zero, ref cds);
            }
            finally
            {
                cds.Dispose();
            }
        }
    }

    public class PlayTrackEventArgs : EventArgs
    {
        public string Filename { get; set; }

        public PlayTrackEventArgs(string filename)
        {
            this.Filename = filename;
        }
    }
}
