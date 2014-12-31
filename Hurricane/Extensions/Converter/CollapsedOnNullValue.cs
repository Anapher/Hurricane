using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hurricane.Extensions.Converter
{
    class CollapsedOnNullValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
