using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Hurricane.GUI.Behaviors
{
    static class FrameworkElementBehavior
    {
        public static readonly DependencyProperty AnimationTriggerProperty = DependencyProperty.RegisterAttached(
            "AnimationTrigger", typeof (object), typeof (FrameworkElementBehavior),
            new PropertyMetadata(default(object), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = (FrameworkElement)dependencyObject;
            Storyboard storyboard = new Storyboard();
            DoubleAnimation da = new DoubleAnimation(0.3, 1, TimeSpan.FromMilliseconds(500));
            ThicknessAnimation ta = new ThicknessAnimation(new Thickness(-10, 0, 10, 0), new Thickness(0), TimeSpan.FromSeconds(0.4));
            Storyboard.SetTarget(da, control);
            Storyboard.SetTarget(ta, control);
            Storyboard.SetTargetProperty(da, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTargetProperty(ta, new PropertyPath(FrameworkElement.MarginProperty));

            storyboard.Children.Add(da);
            storyboard.Children.Add(ta);
            storyboard.Begin(control);
        }

        public static void SetAnimationTrigger(DependencyObject element, object value)
        {
            element.SetValue(AnimationTriggerProperty, value);
        }

        public static object GetAnimationTrigger(DependencyObject element)
        {
            return element.GetValue(AnimationTriggerProperty);
        }
    }
}