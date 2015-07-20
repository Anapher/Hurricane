using System;
using System.Globalization;
using System.Windows.Data;

namespace Hurricane.Converter
{
    [ValueConversion(typeof(object), typeof(bool))]
    internal class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}