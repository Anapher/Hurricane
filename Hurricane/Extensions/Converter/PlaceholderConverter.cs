using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Hurricane.Extensions.Converter
{
    class PlaceholderConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "-";
            if (value.GetType() == typeof(string))
            {
                return string.IsNullOrEmpty(value.ToString()) ? "-" : value;
            }
            else if (value.GetType() == typeof(int))
            {
                return (int)value == 0 ? "-" : value;
            }
            else if (value.GetType() == typeof(uint))
            {
                return (uint)value == 0 ? "-" : value;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
