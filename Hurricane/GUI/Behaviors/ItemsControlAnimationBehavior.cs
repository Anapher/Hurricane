using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Hurricane.Utilities;

namespace Hurricane.GUI.Behaviors
{
    class ItemsControlAnimationBehavior
    {
        public static readonly DependencyProperty IsAnimationEnabledProperty = DependencyProperty.RegisterAttached(
            "IsAnimationEnabled", typeof(bool), typeof(ItemsControlAnimationBehavior), new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var itemsControl = dependencyObject as ItemsControl;
            if (itemsControl == null)
                throw new ArgumentException("The dependencyObject isn't an ItemsControl");

            var dependencyPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl));
            if ((bool)dependencyPropertyChangedEventArgs.NewValue)
            {
                dependencyPropertyDescriptor.AddValueChanged(dependencyObject, ItemsSourceChanged);
            }
            else
            {
                dependencyPropertyDescriptor.RemoveValueChanged(itemsControl, ItemsSourceChanged);
            }
        }

        private static readonly Dictionary<ItemsControl, DispatcherTimer> Timers = new Dictionary<ItemsControl, DispatcherTimer>();

        private async static void ItemsSourceChanged(object sender, EventArgs eventArgs)
        {
            var itemsControl = (ItemsControl)sender;
            if (itemsControl.ItemsSource == null) return;

            //TODO: Hier die Animationen ändern
            var opacityAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(260), FillBehavior.Stop);
            var marginAnimation = new ThicknessAnimation(new Thickness(-20, 0, 20, 0), new Thickness(0),
                TimeSpan.FromMilliseconds(240), FillBehavior.Stop)
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
            };

            if (Timers.ContainsKey(itemsControl))
            {
                Timers[itemsControl].Stop();
                Timers.Remove(itemsControl);
            }

            List<ListBoxItem> visibleItems;

            while (true)
            {
                visibleItems = DependencyObjectExtensions.GetVisibleItemsFromItemsControl(itemsControl,
                    Window.GetWindow(itemsControl));
                if (visibleItems.Count > 0 || itemsControl.Items.Count == 0)
                    break;
                await Task.Delay(1);
            }

            foreach (var item in visibleItems)
            {
                item.Opacity = 0;
            }

            DispatcherTimer dispatcherTimer;

            var enumerator = visibleItems.GetEnumerator();
            if (enumerator.MoveNext())
            {
                ScrollChangedEventHandler scrollChangedEventHandler = null;
                scrollChangedEventHandler = (o, args) =>
                {
                    var handler = scrollChangedEventHandler;

                    if (handler != null)
                        itemsControl.RemoveHandler(ScrollViewer.ScrollChangedEvent, handler);

                    if (!Timers.ContainsKey(itemsControl) || !Timers[itemsControl].IsEnabled)
                        return;

                    var timer = Timers[itemsControl];
                    timer.Stop();

                    while (true)
                    {
                        var item = enumerator.Current;
                        if (item == null)
                            break;
                        item.Opacity = 1;
                        if (!enumerator.MoveNext()) break;
                    }
                };

                dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) }; //TODO: Hier könnt ihr die Zeitspanne zwischen den Animationen für die Element einstellen
                dispatcherTimer.Tick += (s, timerE) =>
                {
                    var item = enumerator.Current; //Bei jedem Tick animieren wir das nächte Item
                    if (item == null) return;
                    item.BeginAnimation(FrameworkElement.MarginProperty, marginAnimation);
                    item.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
                    item.Opacity = 1;
                    if (!enumerator.MoveNext()) //Wenn keines mehr da ist, stoppen wir den Timer und löschen den Handler für das Scroll-Event
                    {
                        dispatcherTimer.Stop();
                        itemsControl.RemoveHandler(ScrollViewer.ScrollChangedEvent, scrollChangedEventHandler);
                    }
                };

                if (Timers.ContainsKey(itemsControl)) Timers.Remove(itemsControl);
                Timers.Add(itemsControl, dispatcherTimer);
                dispatcherTimer.Start();

                itemsControl.AddHandler(ScrollViewer.ScrollChangedEvent, scrollChangedEventHandler);
            }
        }

        public static void SetIsAnimationEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsAnimationEnabledProperty, value);
        }

        public static bool GetIsAnimationEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsAnimationEnabledProperty);
        }
    }
}
