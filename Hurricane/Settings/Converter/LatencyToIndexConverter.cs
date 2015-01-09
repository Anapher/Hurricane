using System;
using System.Globalization;
using System.Windows.Data;

namespace Hurricane.Settings.Converter
{
    class LatencyToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int) value)/50 - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (((int) value) + 1)*50;
        }
    }
}
