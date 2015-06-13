using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Hurricane.Behavior
{
    static class TextBlockBehavior
    {
        public static readonly DependencyProperty UpperTextProperty = DependencyProperty.RegisterAttached(
            "UpperText", typeof (string), typeof (TextBlockBehavior), new PropertyMetadata(string.Empty, UpperTextPropertyChangedCallback));

        public static readonly DependencyProperty InlinesProperty = DependencyProperty.RegisterAttached(
            "Inlines", typeof (IEnumerable<Inline>), typeof (TextBlockBehavior), new PropertyMetadata(default(IEnumerable<Inline>), InlinesPropertyChangedCallback));

        public static void SetInlines(DependencyObject element, IEnumerable<Inline> value)
        {
            element.SetValue(InlinesProperty, value);
        }

        public static IEnumerable<Inline> GetInlines(DependencyObject element)
        {
            return (IEnumerable<Inline>) element.GetValue(InlinesProperty);
        }

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

        private static void InlinesPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var textBlock = dependencyObject as TextBlock;
            if (textBlock != null)
            {
                textBlock.Inlines.Clear();
                var inlines = dependencyPropertyChangedEventArgs.NewValue as IEnumerable<Inline>;
                if (inlines != null)
                    textBlock.Inlines.AddRange(inlines);
            }
        }
    }
}