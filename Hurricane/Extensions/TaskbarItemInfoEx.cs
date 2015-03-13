using System;
using System.Windows;
using System.Windows.Shell;

namespace Hurricane.Extensions
{
    class TaskbarItemInfoEx
    {
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.RegisterAttached(
            "Maximum", typeof (double), typeof (TaskbarItemInfoEx), new PropertyMetadata(default(double), ProgressUpdate));

        private static void ProgressUpdate(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var info = dependencyObject as TaskbarItemInfo;
            if(info == null) throw new ArgumentException();
            info.ProgressValue = GetProgress(dependencyObject) / GetMaximum(dependencyObject);
        }

        public static void SetMaximum(DependencyObject element, double value)
        {
            element.SetValue(MaximumProperty, value);
        }

        public static double GetMaximum(DependencyObject element)
        {
            return (double) element.GetValue(MaximumProperty);
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.RegisterAttached(
            "Progress", typeof(double), typeof(TaskbarItemInfoEx), new PropertyMetadata(default(double), ProgressUpdate));

        public static void SetProgress(DependencyObject element, double value)
        {
            element.SetValue(ProgressProperty, value);
        }

        public static double GetProgress(DependencyObject element)
        {
            return (double) element.GetValue(ProgressProperty);
        }
    }
}
