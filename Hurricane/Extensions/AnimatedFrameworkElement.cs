using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Hurricane.Extensions
{
    public class AnimatedFrameworkElement
    {
        public static readonly DependencyProperty AnimationTriggerProperty = DependencyProperty.RegisterAttached(
            "AnimationTrigger", typeof (object), typeof (AnimatedFrameworkElement), new PropertyMetadata(default(object), PropertyChangedCallback));


        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = (FrameworkElement) dependencyObject;
            Storyboard storyb = new Storyboard();
            DoubleAnimation da = new DoubleAnimation(0.3, 1, TimeSpan.FromMilliseconds(500));
            ThicknessAnimation ta = new ThicknessAnimation(new Thickness(-10, 0, 10, 0), new Thickness(0), TimeSpan.FromSeconds(0.4));
            Storyboard.SetTarget(da, control);
            Storyboard.SetTarget(ta, control);
            Storyboard.SetTargetProperty(da, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTargetProperty(ta, new PropertyPath(FrameworkElement.MarginProperty));

            storyb.Children.Add(da);
            storyb.Children.Add(ta);
            storyb.Begin(control);
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
