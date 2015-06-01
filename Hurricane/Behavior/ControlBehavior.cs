using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hurricane.Behavior
{
    static class ControlBehavior
    {
        public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.RegisterAttached(
            "DoubleClickCommand", typeof (ICommand), typeof (ControlBehavior), new PropertyMetadata(default(ICommand), DoubleClickCommandPropertyChangedCallback));

        public static readonly DependencyProperty DoubleClickCommandParameterProperty = DependencyProperty.RegisterAttached(
            "DoubleClickCommandParameter", typeof (object), typeof (ControlBehavior), new PropertyMetadata(default(object)));

        public static void SetDoubleClickCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(DoubleClickCommandParameterProperty, value);
        }

        public static object GetDoubleClickCommandParameter(DependencyObject element)
        {
            return element.GetValue(DoubleClickCommandParameterProperty);
        }

        public static void SetDoubleClickCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(DoubleClickCommandProperty, value);
        }

        public static ICommand GetDoubleClickCommand(DependencyObject element)
        {
            return (ICommand) element.GetValue(DoubleClickCommandProperty);
        }

        private static void DoubleClickCommandPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = dependencyObject as Control;
            if (control == null)
                throw new ArgumentException(nameof(dependencyObject));

            if (dependencyPropertyChangedEventArgs.NewValue == null &&
                dependencyPropertyChangedEventArgs.OldValue != null)
                control.MouseDoubleClick -= Control_MouseDoubleClick;
            else
                control.MouseDoubleClick += Control_MouseDoubleClick;
        }

        private static void Control_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var control = sender as Control;
            if (control == null)
                return;

            GetDoubleClickCommand(control).Execute(GetDoubleClickCommandParameter(control));
        }
    }
}
