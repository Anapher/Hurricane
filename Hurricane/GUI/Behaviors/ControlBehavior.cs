using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hurricane.GUI.Behaviors
{
    static class ControlBehavior
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(ControlBehavior),
                new PropertyMetadata(OnChangedCommand));

        public static ICommand GetCommand(Control target)
        {
            return (ICommand)target.GetValue(CommandProperty);
        }

        public static void SetCommand(Control target, ICommand value)
        {
            target.SetValue(CommandProperty, value);
        }

        private static void OnChangedCommand(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as Control;
            if (control == null) throw new ArgumentException();
            control.PreviewMouseDoubleClick += Element_PreviewMouseDoubleClick;
        }

        private static void Element_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var control = sender as Control;
            var command = GetCommand(control);

            if (command.CanExecute(null))
            {
                command.Execute(null);
                e.Handled = true;
            }
        }
    }
}