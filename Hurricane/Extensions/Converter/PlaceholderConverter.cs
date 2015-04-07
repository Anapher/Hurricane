using System;
using System.Globalization;
using System.Windows.Data;

namespace Hurricane.Extensions.Converter
{
    class PlaceholderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "-";
            if (value is string)
                return string.IsNullOrEmpty(value.ToString()) ? "-" : value;

            if (value is int)
                return (int)value == 0 ? "-" : value;

            if (value is uint)
                return (uint)value == 0 ? "-" : value;

            if (value is TimeSpan)
                return (TimeSpan) value == TimeSpan.Zero ? "--:--" : value;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
