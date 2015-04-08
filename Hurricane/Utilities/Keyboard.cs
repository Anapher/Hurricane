using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Hurricane.Utilities
{
    public class KeyboardListener : IDisposable
    {
        /// <summary>
        /// Creates global keyboard listener.
        /// </summary>
        public KeyboardListener()
        {
            // We have to store the HookCallback, so that it is not garbage collected runtime
            _hookedLowLevelKeyboardProc = LowLevelKeyboardProc;

            // Set the hook
            _hookId = InterceptKeys.SetHook(_hookedLowLevelKeyboardProc);

            // Assign the asynchronous callback event
            _hookedKeyboardCallbackAsync = KeyboardListener_KeyboardCallbackAsync;
        }

        /// <summary>
        /// Destroys global keyboard listener.
        /// </summary>
        ~KeyboardListener()
        {
            Dispose();
        }

        /// <summary>
        /// Fired when any of the keys is pressed down.
        /// </summary>
        public event RawKeyEventHandler KeyDown;

        /// <summary>
        /// Fired when any of the keys is released.
        /// </summary>
        public event RawKeyEventHandler KeyUp;

        #region Inner workings
        /// <summary>
        /// Hook ID
        /// </summary>
        private readonly IntPtr _hookId;

        /// <summary>
        /// Asynchronous callback hook.
        /// </summary>
        private delegate void KeyboardCallbackAsync(InterceptKeys.KeyEvent keyEvent, int vkCode);

        /// <summary>
        /// Actual callback hook.
        /// 
        /// <remarks>Calls asynchronously the asyncCallback.</remarks>
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private IntPtr LowLevelKeyboardProc(int nCode, UIntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
                if (wParam.ToUInt32() == (int)InterceptKeys.KeyEvent.WM_KEYDOWN ||
                    wParam.ToUInt32() == (int)InterceptKeys.KeyEvent.WM_KEYUP ||
                    wParam.ToUInt32() == (int)InterceptKeys.KeyEvent.WM_SYSKEYDOWN ||
                    wParam.ToUInt32() == (int)InterceptKeys.KeyEvent.WM_SYSKEYUP)
                    _hookedKeyboardCallbackAsync.BeginInvoke((InterceptKeys.KeyEvent)wParam.ToUInt32(), Marshal.ReadInt32(lParam), null, null);

            return InterceptKeys.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// Event to be invoked asynchronously (BeginInvoke) each time key is pressed.
        /// </summary>
        private readonly KeyboardCallbackAsync _hookedKeyboardCallbackAsync;

        /// <summary>
        /// Contains the hooked callback in runtime.
        /// </summary>
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly InterceptKeys.LowLevelKeyboardProc _hookedLowLevelKeyboardProc;

        /// <summary>
        /// HookCallbackAsync procedure that calls accordingly the KeyDown or KeyUp events.
        /// </summary>
        /// <param name="keyEvent">Keyboard event</param>
        /// <param name="vkCode">VKCode</param>
        void KeyboardListener_KeyboardCallbackAsync(InterceptKeys.KeyEvent keyEvent, int vkCode)
        {
            switch (keyEvent)
            {
                // KeyDown events
                case InterceptKeys.KeyEvent.WM_KEYDOWN:
                    if (KeyDown != null)
                        KeyDown(this, new RawKeyEventArgs(vkCode, false));
                    break;
                case InterceptKeys.KeyEvent.WM_SYSKEYDOWN:
                    if (KeyDown != null)
                        KeyDown(this, new RawKeyEventArgs(vkCode, true));
                    break;

                // KeyUp events
                case InterceptKeys.KeyEvent.WM_KEYUP:
                    if (KeyUp != null)
                        KeyUp(this, new RawKeyEventArgs(vkCode, false));
                    break;
                case InterceptKeys.KeyEvent.WM_SYSKEYUP:
                    if (KeyUp != null)
                        KeyUp(this, new RawKeyEventArgs(vkCode, true));
                    break;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes the hook.
        /// <remarks>This call is required as it calls the UnhookWindowsHookEx.</remarks>
        /// </summary>
        public void Dispose()
        {
            InterceptKeys.UnhookWindowsHookEx(_hookId);
        }

        #endregion
    }
    /// <summary>
    /// Raw KeyEvent arguments.
    /// </summary>
    public class RawKeyEventArgs : EventArgs
    {
        /// <summary>
        /// VKCode of the key.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public int VKCode;

        /// <summary>
        /// WPF Key of the key.
        /// </summary>
        public Key Key;

        /// <summary>
        /// Is the hitted key system key.
        /// </summary>
        public bool IsSysKey;

        /// <summary>
        /// Create raw keyevent arguments.
        /// </summary>
        /// <param name="vkCode"></param>
        /// <param name="isSysKey"></param>
        public RawKeyEventArgs(int vkCode, bool isSysKey)
        {
            VKCode = vkCode;
            IsSysKey = isSysKey;
            Key = KeyInterop.KeyFromVirtualKey(vkCode);
        }
    }

    /// <summary>
    /// Raw keyevent handler.
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="args">raw keyevent arguments</param>
    public delegate void RawKeyEventHandler(object sender, RawKeyEventArgs args);

    #region WINAPI Helper class
    /// <summary>
    /// Winapi Key interception helper class.
    /// </summary>
    internal static class InterceptKeys
    {
        public delegate IntPtr LowLevelKeyboardProc(int nCode, UIntPtr wParam, IntPtr lParam);
        // ReSharper disable once InconsistentNaming
        public static int WH_KEYBOARD_LL = 13;

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum KeyEvent
        {
            WM_KEYDOWN = 256,
            WM_KEYUP = 257,
            WM_SYSKEYUP = 261,
            WM_SYSKEYDOWN = 260
        }

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, UIntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }
    #endregion
}