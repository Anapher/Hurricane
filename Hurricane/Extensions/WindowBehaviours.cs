using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hurricane.Extensions
{
    public static class WindowBehaviours
    {
        // Closing window
        public static void SetClose(DependencyObject target, bool value)
        {
            target.SetValue(CloseProperty, value);
        }

        public static readonly DependencyProperty CloseProperty =
                                                  DependencyProperty.RegisterAttached("Close",
                                                  typeof(bool),
                                                  typeof(WindowBehaviours),
                                                  new UIPropertyMetadata(false, OnClose));

        public static readonly DependencyProperty DragMove =
                                          DependencyProperty.RegisterAttached("DragMove",
                                          typeof(bool),
                                          typeof(WindowBehaviours),
                                          new UIPropertyMetadata(false, OnDragMove));

        private static void OnClose(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool && ((bool)e.NewValue))
            {
                Window window = GetWindow(sender);

                if (window != null)
                {
                    window.Close();
                }
            }
        }

        private static void OnDragMove(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool && ((bool)e.NewValue))
            {
                Window window = GetWindow(sender);

                if (window != null)
                {
                    window.DragMove();
                }
            }
        }


        private static Window GetWindow(DependencyObject sender)
        {
            Window window = null;

            if (sender is Window)
            {
                window = (Window)sender;
            }

            if (window == null)
            {
                window = Window.GetWindow(sender);
            }

            return window;
        }
    }
}
