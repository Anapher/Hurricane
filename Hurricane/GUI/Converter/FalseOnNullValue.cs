using System;
using System.Globalization;
using System.Windows.Data;

namespace Hurricane.GUI.Converter
{
    class FalseOnNullValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
