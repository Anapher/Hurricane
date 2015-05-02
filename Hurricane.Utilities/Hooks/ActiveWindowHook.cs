using System;
using Hurricane.Utilities.Native;

namespace Hurricane.Utilities.Hooks
{
    public class ActiveWindowHook : IDisposable
    {
        const uint EVENT_SYSTEM_FOREGROUND = 3;
        const uint WINEVENT_OUTOFCONTEXT = 0;

        private readonly NativeDelegates.WinEventDelegate _winEventProc;
        private IntPtr _hhook;

        public ActiveWindowHook()
        {
            _winEventProc = WinEventProc;
        }

        public void Dispose()
        {
            Disable();
        }

        public delegate void ActiveWindowChangedHandler(object sender, IntPtr hwnd);
        public event ActiveWindowChangedHandler ActiveWindowChanged;

        public bool IsEnabled { get; set; }

        public void Enable()
        {
            if (IsEnabled) return;
            IsEnabled = true;
            _hhook = UnsafeNativeMethods.SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        public void Disable()
        {
            if (!IsEnabled) return;
            IsEnabled = false;
            UnsafeNativeMethods.UnhookWinEvent(_hhook);
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType == EVENT_SYSTEM_FOREGROUND)
            {
                if (ActiveWindowChanged != null)
                    ActiveWindowChanged(this, hwnd);
            }
        }
    }
}