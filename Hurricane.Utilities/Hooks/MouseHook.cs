using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Hurricane.Utilities.Native;

namespace Hurricane.Utilities.Hooks
{
    public class MouseHook : IDisposable
    {
        private IntPtr _mouseHookHandle;
        private NativeDelegates.HookProc _mouseDelegate;
        private int _oldX;
        private int _oldY;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                Disable();
            }
            // free native resources if there are any.
        }

        public event EventHandler<MouseMoveEventArgs> MouseMove;

        public bool IsEnabled { get; set; }

        public void Enable()
        {
            if (IsEnabled) return;
            IsEnabled = true;
            _mouseDelegate = MouseHookProc;
            _mouseHookHandle = UnsafeNativeMethods.SetWindowsHookEx(
                Enums.HookType.WH_MOUSE_LL,
                _mouseDelegate,
                IntPtr.Zero,
                0);
        }

        public void Disable()
        {
            if (!IsEnabled) return;
            IsEnabled = false;
            var result = UnsafeNativeMethods.UnhookWindowsHookEx(_mouseHookHandle);
            _mouseHookHandle = IntPtr.Zero;
            _mouseDelegate = null;
            if (!result)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode);
            }
        }

        private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var mouseHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));
            if (MouseMove != null && (_oldX != mouseHookStruct.Point.X || _oldY != mouseHookStruct.Point.Y))
            {
                _oldX = mouseHookStruct.Point.X;
                _oldY = mouseHookStruct.Point.Y;
                MouseMove.Invoke(this, new MouseMoveEventArgs(mouseHookStruct.Point.X, mouseHookStruct.Point.Y));
            }

            return UnsafeNativeMethods.CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
        }
    }

    /// <summary>
    /// Provides data for the MouseMove event. It also provides a property Handled.
    /// Set this property to <b>true</b> to prevent further processing of the event in other applications.
    /// </summary>
    public class MouseMoveEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the x-coordinate of the mouse during the generating mouse event.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets the y-coordinate of the mouse during the generating mouse event.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Set this property to <b>true</b> inside your event handler to prevent further processing of the event in other applications.
        /// </summary>
        public bool Handled { get; set; }

        public MouseMoveEventArgs(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}