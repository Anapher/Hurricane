using System.Windows;
using System.Windows.Media;

namespace Hurricane.Helpers
{
    static class FindVisualChildHelper
    {
        public static T GetFirstChildOfType<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null)
            {
                return null;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);

                var result = (child as T) ?? GetFirstChildOfType<T>(child);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
