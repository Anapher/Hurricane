using System.Windows;
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
    }
}
