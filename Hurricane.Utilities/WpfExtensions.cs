using System.Windows;
using System.Windows.Media;

namespace Hurricane.Utilities
{
    public static class WpfExtensions
    {
        /// <summary>
        /// Searches for a visual parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="child">The child of the parent</param>
        /// <returns>The parent of the child</returns>
        public static T GetVisualParent<T>(this DependencyObject child) where T : Visual
        {
            while ((child != null) && !(child is T))
                child = VisualTreeHelper.GetParent(child);
            
            return (T)child;
        }
    }
}
