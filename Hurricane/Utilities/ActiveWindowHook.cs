using System;
using Hurricane.Utilities.Native;

namespace Hurricane.Utilities
{
    class ActiveWindowHook : IDisposable
    {
        public delegate void ActiveWindowChangedHandler(object sender, IntPtr hwnd);
        public event ActiveWindowChangedHandler ActiveWindowChanged;

        const uint WINEVENT_OUTOFCONTEXT = 0;
        const uint EVENT_SYSTEM_FOREGROUND = 3;

        IntPtr m_hhook;
        private readonly WinEventDelegate _winEventProc;

        public ActiveWindowHook()
        {
            _winEventProc = new WinEventDelegate(WinEventProc);
        }

        public void Hook()
        {
            m_hhook = UnsafeNativeMethods.SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType == EVENT_SYSTEM_FOREGROUND)
            {
                if (ActiveWindowChanged != null)
                    ActiveWindowChanged(this, hwnd);
            }
        }

        public void RaiseOne()
        {
            if (ActiveWindowChanged != null)
                ActiveWindowChanged(this, UnsafeNativeMethods.GetForegroundWindow());
        }

        public void Unhook()
        {
            UnsafeNativeMethods.UnhookWinEvent(m_hhook);
        }

        public void Dispose()
        {
            Unhook();
        }
    }
}
