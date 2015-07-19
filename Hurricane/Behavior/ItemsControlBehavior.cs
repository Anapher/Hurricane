using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Hurricane.Behavior
{
    static class ItemsControlBehavior
    {
        public static readonly DependencyProperty ItemsSourceChangedAnimationProperty = DependencyProperty.RegisterAttached(
            "ItemsSourceChangedAnimation", typeof (Storyboard), typeof (ItemsControlBehavior), new PropertyMetadata(default(Storyboard), ItemsSourceChangedAnimationPropertyChangedCallback));

        public static void SetItemsSourceChangedAnimation(DependencyObject element, Storyboard value)
        {
            element.SetValue(ItemsSourceChangedAnimationProperty, value);
        }

        public static Storyboard GetItemsSourceChangedAnimation(DependencyObject element)
        {
            return (Storyboard) element.GetValue(ItemsSourceChangedAnimationProperty);
        }

        private static void ItemsSourceChangedAnimationPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var itemsControl = dependencyObject as ItemsControl;
            if (itemsControl == null)
                throw new InvalidOperationException("Can only be applied to an ItemsControl");

            var propertyDescriptor = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty,
                typeof (ItemsControl));

            propertyDescriptor.RemoveValueChanged(itemsControl, ItemsSourceChangedHandler);
            propertyDescriptor.AddValueChanged(itemsControl, ItemsSourceChangedHandler);
        }

        private static void ItemsSourceChangedHandler(object sender, EventArgs eventArgs)
        {
            var animateObject = (ItemsControl) sender;
            var storyboard = GetItemsSourceChangedAnimation(animateObject);
            storyboard.Begin(animateObject);
        }
    }
}