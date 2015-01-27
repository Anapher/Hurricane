using System;
using System.Windows.Data;

namespace Hurricane.Extensions.Converter
{
    class FormatLargeNumber : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var n = uint.Parse(value.ToString());
            return n >= 10000 ? n.ToString("n0") : n.ToString("d");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
