using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hurricane.Utilities
{
    public static class DependencyObjectExtensions
    {
        public static T GetVisualParent<T>(this DependencyObject child) where T : Visual
        {
            while ((child != null) && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return (T) child;
        }

        private static bool IsUserVisible(FrameworkElement element, FrameworkElement container)
        {
            if (!element.IsVisible)
                return false;

            Rect bounds =
                element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            var rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
        }

        public static List<ListViewItem> GetVisibleItemsFromListView(ListView listView, FrameworkElement parentToTestVisibility)
        {
            var items = new List<ListViewItem>();

            foreach (var item in listView.ItemsSource)
            {
                var lvItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromItem(item);
                if (lvItem == null)
                    continue;

                if (IsUserVisible(lvItem, parentToTestVisibility))
                {
                    items.Add(lvItem);
                }
                else if (items.Any())
                {
                    break;
                }
            }

            return items;
        }
    }
}
