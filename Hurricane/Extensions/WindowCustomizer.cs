using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Extensions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;

    public static class WindowCustomizer
    {
        #region CanMaximize
        public static readonly DependencyProperty CanMaximize =
            DependencyProperty.RegisterAttached("CanMaximize", typeof(bool), typeof(WindowCustomizer),
                new PropertyMetadata(true, new PropertyChangedCallback(OnCanMaximizeChanged)));
        private static void OnCanMaximizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = d as Window;
            if (window != null)
            {
                RoutedEventHandler loadedHandler = null;
                loadedHandler = delegate
                {
                    if ((bool)e.NewValue)
                    {
                        WindowHelper.EnableMaximize(window);
                    }
                    else
                    {
                        WindowHelper.DisableMaximize(window);
                    }
                    window.Loaded -= loadedHandler;
                };

                if (!window.IsLoaded)
                {
                    window.Loaded += loadedHandler;
                }
                else
                {
                    loadedHandler(null, null);
                }
            }
        }
        public static void SetCanMaximize(DependencyObject d, bool value)
        {
            d.SetValue(CanMaximize, value);
        }
        public static bool GetCanMaximize(DependencyObject d)
        {
            return (bool)d.GetValue(CanMaximize);
        }
        #endregion CanMaximize

        #region CanMinimize
        public static readonly DependencyProperty CanMinimize =
            DependencyProperty.RegisterAttached("CanMinimize", typeof(bool), typeof(WindowCustomizer),
                new PropertyMetadata(true, new PropertyChangedCallback(OnCanMinimizeChanged)));
        private static void OnCanMinimizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = d as Window;
            if (window != null)
            {
                RoutedEventHandler loadedHandler = null;
                loadedHandler = delegate
                {
                    if ((bool)e.NewValue)
                    {
                        WindowHelper.EnableMinimize(window);
                    }
                    else
                    {
                        WindowHelper.DisableMinimize(window);
                    }
                    window.Loaded -= loadedHandler;
                };

                if (!window.IsLoaded)
                {
                    window.Loaded += loadedHandler;
                }
                else
                {
                    loadedHandler(null, null);
                }
            }
        }
        public static void SetCanMinimize(DependencyObject d, bool value)
        {
            d.SetValue(CanMinimize, value);
        }
        public static bool GetCanMinimize(DependencyObject d)
        {
            return (bool)d.GetValue(CanMinimize);
        }
        #endregion CanMinimize

        #region WindowHelper Nested Class
        public static class WindowHelper
        {
            private const Int32 GWL_STYLE = -16;
            private const Int32 WS_MAXIMIZEBOX = 0x00010000;
            private const Int32 WS_MINIMIZEBOX = 0x00020000;

            [DllImport("User32.dll", EntryPoint = "GetWindowLong")]
            private extern static Int32 GetWindowLongPtr(IntPtr hWnd, Int32 nIndex);

            [DllImport("User32.dll", EntryPoint = "SetWindowLong")]
            private extern static Int32 SetWindowLongPtr(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong);

            /// <summary>
            /// Disables the maximize functionality of a WPF window.
            /// </summary>
            ///The WPF window to be modified.
            public static void DisableMaximize(Window window)
            {
                lock (window)
                {
                    IntPtr hWnd = new WindowInteropHelper(window).Handle;
                    Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
                    SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle & ~WS_MAXIMIZEBOX);
                }
            }

            /// <summary>
            /// Disables the minimize functionality of a WPF window.
            /// </summary>
            ///The WPF window to be modified.
            public static void DisableMinimize(Window window)
            {
                lock (window)
                {
                    IntPtr hWnd = new WindowInteropHelper(window).Handle;
                    Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
                    SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle & ~WS_MINIMIZEBOX);
                }
            }

            /// <summary>
            /// Enables the maximize functionality of a WPF window.
            /// </summary>
            ///The WPF window to be modified.
            public static void EnableMaximize(Window window)
            {
                lock (window)
                {
                    IntPtr hWnd = new WindowInteropHelper(window).Handle;
                    Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
                    SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle | WS_MAXIMIZEBOX);
                }
            }

            /// <summary>
            /// Enables the minimize functionality of a WPF window.
            /// </summary>
            ///The WPF window to be modified.
            public static void EnableMinimize(Window window)
            {
                lock (window)
                {
                    IntPtr hWnd = new WindowInteropHelper(window).Handle;
                    Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
                    SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle | WS_MINIMIZEBOX);
                }
            }

            /// <summary>
            /// Toggles the enabled state of a WPF window's maximize functionality.
            /// </summary>
            ///The WPF window to be modified.
            public static void ToggleMaximize(Window window)
            {
                lock (window)
                {
                    IntPtr hWnd = new WindowInteropHelper(window).Handle;
                    Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);

                    if ((windowStyle | WS_MAXIMIZEBOX) == windowStyle)
                    {
                        SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle & ~WS_MAXIMIZEBOX);
                    }
                    else
                    {
                        SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle | WS_MAXIMIZEBOX);
                    }
                }
            }

            /// <summary>
            /// Toggles the enabled state of a WPF window's minimize functionality.
            /// </summary>
            ///The WPF window to be modified.
            public static void ToggleMinimize(Window window)
            {
                lock (window)
                {
                    IntPtr hWnd = new WindowInteropHelper(window).Handle;
                    Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);

                    if ((windowStyle | WS_MINIMIZEBOX) == windowStyle)
                    {
                        SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle & ~WS_MINIMIZEBOX);
                    }
                    else
                    {
                        SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle | WS_MINIMIZEBOX);
                    }
                }
            }
        }
        #endregion WindowHelper Nested Class
    }
}