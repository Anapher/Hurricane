using System;
using System.Globalization;
using System.Windows.Data;

namespace Hurricane.Converter
{
    class SizeToColumnsConverter : IValueConverter
    {
        public double MaxSize { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = (double) value;
            return (int) (size/MaxSize);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
