using System;

namespace Hurricane.Utilities.Native
{
    public static class NativeDelegates
    {
        public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);
        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
    }
}
