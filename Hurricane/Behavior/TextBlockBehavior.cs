using System;
using System.Windows;
using System.Windows.Controls;

namespace Hurricane.Behavior
{
    static class TextBlockBehavior
    {
        public static readonly DependencyProperty UpperTextProperty = DependencyProperty.RegisterAttached(
            "UpperText", typeof (string), typeof (TextBlockBehavior), new PropertyMetadata(string.Empty, UpperTextPropertyChangedCallback));

        public static void SetUpperText(DependencyObject element, string value)
        {
            element.SetValue(UpperTextProperty, value);
        }

        public static string GetUpperText(DependencyObject element)
        {
            return (string) element.GetValue(UpperTextProperty);
        }

        private static void UpperTextPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var element = dependencyObject as TextBlock;
            if (element == null) throw new ArgumentException(nameof(dependencyObject));
            element.Text = dependencyPropertyChangedEventArgs.NewValue.ToString().ToUpper();
        }
    }
}