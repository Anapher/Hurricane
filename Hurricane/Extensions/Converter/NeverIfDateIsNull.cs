using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Hurricane.Extensions.Converter
{
    class NeverIfDateIsNull : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || ((DateTime)value).Year == 1)
            {
                return Application.Current.FindResource("never").ToString();
            }
            else { return ((DateTime)value).ToString(Application.Current.FindResource("DateFormat").ToString()); }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
