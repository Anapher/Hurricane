using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Hurricane.Extensions.MahApps
{
    public class TransitioningExtensions
    {
        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.RegisterAttached(
            "DisplayText", typeof(object), typeof(TransitioningExtensions), new PropertyMetadata(default(object), DisplayTextPropertyChangedCallback));

        private static void DisplayTextPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = dependencyObject as TransitioningContentControl;
            if (control == null) throw new ArgumentException();
            control.Content = new TextBlock { Text = dependencyPropertyChangedEventArgs.NewValue.ToString(), SnapsToDevicePixels = true, TextTrimming = TextTrimming.CharacterEllipsis, Margin = new Thickness(1), VerticalAlignment = VerticalAlignment.Center, Foreground = (Brush)Application.Current.Resources["BlackBrush"] };
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
            "DisplayImage", typeof (BitmapSource), typeof (TransitioningExtensions), new PropertyMetadata(default(BitmapSource), DisplayImagePropertyChangedCallback));

        private static void DisplayImagePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = dependencyObject as TransitioningContentControl;
            if (control == null) throw new ArgumentException();
            control.Content = new Image {Source = (BitmapImage) dependencyPropertyChangedEventArgs.NewValue};
        }

        public static void SetDisplayImage(DependencyObject element, BitmapSource value)
        {
            element.SetValue(DisplayImageProperty, value);
        }

        public static BitmapSource GetDisplayImage(DependencyObject element)
        {
            return (BitmapSource) element.GetValue(DisplayImageProperty);
        }
    }
}