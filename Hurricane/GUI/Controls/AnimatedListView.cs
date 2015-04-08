using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Hurricane.Settings;
using Hurricane.Utilities;

namespace Hurricane.GUI.Controls
{
    public class AnimatedListView : ListView
    {
        public static readonly DependencyProperty TransitionProperty = DependencyProperty.Register(
            "Transition", typeof(TrackListAnimation), typeof(AnimatedListView), new PropertyMetadata(default(TrackListAnimation)));

        public TrackListAnimation Transition
        {
            get { return (TrackListAnimation)GetValue(TransitionProperty); }
            set { SetValue(TransitionProperty, value); }
        }

        public AnimatedListView()
        {
            DependencyPropertyDescriptor.FromProperty(ItemsSourceProperty, typeof(ListView)).AddValueChanged(this, ItemsSourceChanged);
        }

        private Thickness? _currentMargin;
        private DispatcherTimer _dispatcherTimer;

        private async void ItemsSourceChanged(object sender, EventArgs eventArgs)
        {
            if (!_currentMargin.HasValue) _currentMargin = Margin;

            switch (Transition)
            {
                case TrackListAnimation.FadeList:
                    Storyboard storyb = new Storyboard();
                    DoubleAnimation da = new DoubleAnimation(0.3, 1, TimeSpan.FromMilliseconds(500));
                    ThicknessAnimation ta = new ThicknessAnimation(new Thickness(_currentMargin.Value.Left - 10, 0, _currentMargin.Value.Right + 10, 0), _currentMargin.Value, TimeSpan.FromSeconds(0.4));
                    Storyboard.SetTarget(da, this);
                    Storyboard.SetTarget(ta, this);
                    Storyboard.SetTargetProperty(da, new PropertyPath(OpacityProperty));
                    Storyboard.SetTargetProperty(ta, new PropertyPath(MarginProperty));

                    storyb.Children.Add(da);
                    storyb.Children.Add(ta);
                    storyb.Begin(this);
                    break;

                case TrackListAnimation.FadeEveryItem:
                    var opacityAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(255), FillBehavior.Stop);
                    var marginAnimation = new ThicknessAnimation(new Thickness(-20, 0, 20, 0), new Thickness(0), TimeSpan.FromMilliseconds(200), FillBehavior.Stop)
                    {
                        EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
                    };

                    if (_dispatcherTimer != null) _dispatcherTimer.Stop();

                                List<ListViewItem> visibleItems;

                    while (true)
                    {
                        visibleItems = DependencyObjectExtensions.GetVisibleItemsFromListView(this,
                            Window.GetWindow(this));
                        if (visibleItems.Count > 0 || Items.Count == 0)
                            break;
                        await Task.Delay(1);
                    }

                    foreach (var item in visibleItems)
                    {
                        item.Opacity = 0;
                    }

                    var enumerator = visibleItems.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        ScrollChangedEventHandler scrollChangedEventHandler = null;
                        scrollChangedEventHandler = (o, args) =>
                        {
                            if (!_dispatcherTimer.IsEnabled) return;
                            _dispatcherTimer.Stop();
                            if (scrollChangedEventHandler != null)
                                RemoveHandler(ScrollViewer.ScrollChangedEvent, scrollChangedEventHandler);
                            while (true)
                            {
                                var item = enumerator.Current;
                                if (item == null)
                                    break;
                                item.Opacity = 1;
                                if (!enumerator.MoveNext()) break;
                            }
                        };

                        _dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(25) };
                        _dispatcherTimer.Tick += (s, timerE) =>
                        {
                            var item = enumerator.Current;
                            if (item == null) return;
                            item.BeginAnimation(MarginProperty, marginAnimation);
                            item.BeginAnimation(OpacityProperty, opacityAnimation);
                            item.Opacity = 1;
                            if (!enumerator.MoveNext())
                            {
                                _dispatcherTimer.Stop();
                                RemoveHandler(ScrollViewer.ScrollChangedEvent, scrollChangedEventHandler);
                            }
                        };

                        _dispatcherTimer.Start();

                        AddHandler(ScrollViewer.ScrollChangedEvent, scrollChangedEventHandler);
                    }
                    break;
            }
        }
    }
}