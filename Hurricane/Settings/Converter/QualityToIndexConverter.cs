using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Hurricane.Settings.Converter
{
    class QualityToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((ImageQuality)value)
            {
                case ImageQuality.small:
                    return 0;
                case ImageQuality.medium :
                    return 1;
                case ImageQuality.large:
                    return 2;
                case ImageQuality.maximum:
                    return 3;
            }
            throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (ImageQuality)(int)value;
        }
    }
}
