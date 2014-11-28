using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Utilities
{
    class ActiveWindowHook : IDisposable
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        public delegate void ActiveWindowChangedHandler(object sender, IntPtr hwnd);
        public event ActiveWindowChangedHandler ActiveWindowChanged;
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        const uint WINEVENT_OUTOFCONTEXT = 0;
        const uint EVENT_SYSTEM_FOREGROUND = 3;

        IntPtr m_hhook;
        private WinEventDelegate _winEventProc;

        public ActiveWindowHook()
        {
            _winEventProc = new WinEventDelegate(WinEventProc);
        }

        public void Hook()
        {
            m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType == EVENT_SYSTEM_FOREGROUND)
            {
                if (ActiveWindowChanged != null)
                    ActiveWindowChanged(this, hwnd);
            }
        }

        public void Unhook()
        {
            UnhookWinEvent(m_hhook);
        }

        public void Dispose()
        {
            Unhook();
        }
    }
}
