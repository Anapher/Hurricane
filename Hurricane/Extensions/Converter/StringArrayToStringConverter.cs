using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Hurricane.Extensions.Converter
{
    class StringArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString())) return string.Empty;

            return string.Join(", ", (string[])value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString())) return null;
            return value.ToString().Split(new string[] { ",", ", " }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
