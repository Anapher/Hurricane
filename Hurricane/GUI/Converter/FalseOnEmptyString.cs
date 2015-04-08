using System;
using System.Globalization;
using System.Windows.Data;

namespace Hurricane.GUI.Converter
{
    class FalseOnEmptyString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            return !string.IsNullOrWhiteSpace(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
