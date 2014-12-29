using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
