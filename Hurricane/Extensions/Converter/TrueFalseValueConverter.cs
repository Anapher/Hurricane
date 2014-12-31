using System;
using System.Globalization;
using System.Windows.Data;

namespace Hurricane.Extensions.Converter
{
    class TrueFalseValueConverter : IValueConverter
    {
        public object TrueValue { get; set; }
        public object FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value) { return TrueValue; }
            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
