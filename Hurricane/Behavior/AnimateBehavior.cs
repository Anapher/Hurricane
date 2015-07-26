using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

namespace Hurricane.Behavior
{
    internal class AnimateBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty AnimationProperty = DependencyProperty.Register(
            "Animation", typeof (Storyboard), typeof (AnimateBehavior), new PropertyMetadata(default(Storyboard)));

        public Storyboard Animation
        {
            get { return (Storyboard) GetValue(AnimationProperty); }
            set { SetValue(AnimationProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof (object), typeof (AnimateBehavior),
            new PropertyMetadata(default(object), ValueChangedCallback));

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void ValueChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var item = dependencyObject as AnimateBehavior;
            if (item?.AssociatedObject == null)
                return;

            item.Animation?.Begin(item.AssociatedObject);
        }
    }
}
