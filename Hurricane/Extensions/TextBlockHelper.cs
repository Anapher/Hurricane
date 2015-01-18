using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Hurricane.Extensions
{
    public class TextBlockHelper
    {
        public static string GetUpperText(DependencyObject obj) { return (string)obj.GetValue(UpperTextProperty); }
        public static void SetUpperText(DependencyObject obj, string value) { obj.SetValue(UpperTextProperty, value); }

        public static readonly DependencyProperty UpperTextProperty = DependencyProperty.RegisterAttached("UpperText", typeof(string), typeof(TextBlockHelper), new UIPropertyMetadata(string.Empty, OnUpperTextChanged));

        private static void OnUpperTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var element = obj as TextBlock;
            if (element == null) throw new ArgumentException();
            element.Text = e.NewValue.ToString().ToUpper();
        }

        public static readonly DependencyProperty PlaceHolderTextProperty = DependencyProperty.RegisterAttached(
            "PlaceHolderText", typeof (string), typeof (TextBlockHelper), new PropertyMetadata(default(string)));

        public static void SetPlaceHolderText(DependencyObject element, string value)
        {
            element.SetValue(PlaceHolderTextProperty, value);
            var txt = element as TextBlock;
            if (txt == null) throw new ArgumentException();
            DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock)).RemoveValueChanged(txt, TextChangedHandler);
            DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock)).AddValueChanged(txt, TextChangedHandler);
            TextChangedHandler(element, EventArgs.Empty);
        }

        private static void TextChangedHandler(object sender, EventArgs eventArgs)
        {
            var txt = (TextBlock) sender;
            if (string.IsNullOrEmpty(txt.Text))
                txt.Text = GetPlaceHolderText(txt);
        }

        public static string GetPlaceHolderText(DependencyObject element)
        {
            return (string) element.GetValue(PlaceHolderTextProperty);
        }
    }
}
