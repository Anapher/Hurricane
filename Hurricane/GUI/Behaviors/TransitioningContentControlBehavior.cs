using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;

namespace Hurricane.GUI.Behaviors
{
    class TransitioningContentControlBehavior
    {
        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.RegisterAttached(
            "DisplayText", typeof (object), typeof (TransitioningContentControlBehavior),
            new PropertyMetadata(default(object), DisplayTextPropertyChangedCallback));

        private static void DisplayTextPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = dependencyObject as TransitioningContentControl;
            if (control == null) throw new ArgumentException();
            control.Content = dependencyPropertyChangedEventArgs.NewValue == null
                ? null
                : new TextBlock
                {
                    Text = dependencyPropertyChangedEventArgs.NewValue.ToString(),
                    SnapsToDevicePixels = true,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(1),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = (Brush)Application.Current.Resources["BlackBrush"]
                };
        }

        public static void SetDisplayText(DependencyObject element, object value)
        {
            element.SetValue(DisplayTextProperty, value);
        }

        public static object GetDisplayText(DependencyObject element)
        {
            return element.GetValue(DisplayTextProperty);
        }

        public static readonly DependencyProperty DisplayImageProperty = DependencyProperty.RegisterAttached(
            "DisplayImage", typeof (BitmapSource), typeof (TransitioningContentControlBehavior),
            new PropertyMetadata(default(BitmapSource), DisplayImagePropertyChangedCallback));

        private static void DisplayImagePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = dependencyObject as TransitioningContentControl;
            if (control == null) throw new ArgumentException();
            control.Content = new Image { Source = (BitmapImage)dependencyPropertyChangedEventArgs.NewValue };
        }

        public static void SetDisplayImage(DependencyObject element, BitmapSource value)
        {
            element.SetValue(DisplayImageProperty, value);
        }

        public static BitmapSource GetDisplayImage(DependencyObject element)
        {
            return (BitmapSource)element.GetValue(DisplayImageProperty);
        }
    }
}