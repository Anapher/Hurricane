using System;
using System.Windows;
using System.Windows.Controls;

namespace Hurricane.GUI.Behaviors
{
    static class ButtonBehavior
    {
        public static readonly DependencyProperty DialogResultProperty = DependencyProperty.RegisterAttached(
            "DialogResult", typeof (bool?), typeof (ButtonBehavior),
            new PropertyMetadata(null, DialogPropertyChangedCallback));

        private static void DialogPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var button = dependencyObject as Button;
            if (button == null)
                throw new InvalidOperationException(
                    "Can only use ButtonBehavior.DialogResult on a Button control");
            button.Click += (sender, e) =>
            {
                var window = Window.GetWindow(button);
                if (window != null) window.DialogResult = GetDialogResult(button);
            };
        }

        public static void SetDialogResult(DependencyObject element, bool? value)
        {
            element.SetValue(DialogResultProperty, value);
        }

        public static bool? GetDialogResult(DependencyObject element)
        {
            return (bool?)element.GetValue(DialogResultProperty);
        }
    }
}