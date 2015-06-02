using System;
using System.Globalization;
using System.Windows.Data;

namespace Hurricane.Converter
{
    [ValueConversion(typeof(string), typeof(bool))]
    class FalseIfStringIsEmpty : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}