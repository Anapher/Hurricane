using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Hurricane.Extensions
{
    public class AnimatedListView : ListView
    {
        public AnimatedListView()
        {
            DependencyPropertyDescriptor.FromProperty(ItemsSourceProperty, typeof(ListView)).AddValueChanged(this, ItemsSourceChanged);
        }

        private Thickness? _currentMargin;

        private void ItemsSourceChanged(object sender, EventArgs eventArgs)
        {
            if (!_currentMargin.HasValue) _currentMargin = this.Margin;

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
        }
    }
}
