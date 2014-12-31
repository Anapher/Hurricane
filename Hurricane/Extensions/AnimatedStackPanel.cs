using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Hurricane.Extensions
{
    public class AnimatedStackPanel : StackPanel
    {
        public static readonly DependencyProperty UpdateObjectProperty = DependencyProperty.Register(
            "UpdateObject", typeof(object), typeof(AnimatedStackPanel), new PropertyMetadata(default(object), PropertyCallback));

        public static readonly DependencyProperty AnimationIntervalProperty = DependencyProperty.Register(
            "AnimationInterval", typeof(int), typeof(AnimatedStackPanel), new PropertyMetadata(default(int)));

        public int AnimationInterval
        {
            get { return (int)GetValue(AnimationIntervalProperty); }
            set { SetValue(AnimationIntervalProperty, value); }
        }

        public object UpdateObject
        {
            get { return GetValue(UpdateObjectProperty); }
            set { SetValue(UpdateObjectProperty, value); }
        }

        private static Storyboard _story;
        private static void PropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var stackPanel = (AnimatedStackPanel) d;
            if (_story != null){ _story.Stop(stackPanel); }
            _story = FadeInAnimation(stackPanel.AnimationInterval, stackPanel.Children.OfType<FrameworkElement>().ToArray());
            _story.Begin(stackPanel, true);
        }

        private static Storyboard FadeInAnimation(int interval, params FrameworkElement[] controls)
        {
            Storyboard fadeInAnimation = new Storyboard();
            int counter = 0;
            foreach (var control in controls)
            {
                control.BeginAnimation(OpacityProperty, null);
                control.BeginAnimation(MarginProperty, null);
                control.Opacity = 0;
                control.Margin = new Thickness(0, control.Margin.Top, 0, 0);
                DoubleAnimation da = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                ThicknessAnimation ta = new ThicknessAnimation(new Thickness(-10, control.Margin.Top, 10, 0), new Thickness(0, control.Margin.Top, 0, 0), TimeSpan.FromMilliseconds(400));
                Storyboard.SetTarget(da, control);
                Storyboard.SetTarget(ta, control);
                Storyboard.SetTargetProperty(da, new PropertyPath(OpacityProperty));
                Storyboard.SetTargetProperty(ta, new PropertyPath(MarginProperty));
                fadeInAnimation.Children.Add(da);
                fadeInAnimation.Children.Add(ta);
                da.BeginTime = TimeSpan.FromMilliseconds(counter * interval);
                ta.BeginTime = TimeSpan.FromMilliseconds(counter * interval);
                counter++;
            }

            fadeInAnimation.Completed += (s, e) =>
            {
                foreach (var c in controls)
                {
                    c.Opacity = 1;
                }
            };
            return fadeInAnimation;
        }
    }
}
