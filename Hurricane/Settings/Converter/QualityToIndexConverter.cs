using System;
using System.Globalization;
using System.Windows.Data;

namespace Hurricane.Settings.Converter
{
    class QualityToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ImageQuality)value)
            {
                case ImageQuality.Small:
                    return 0;
                case ImageQuality.Medium :
                    return 1;
                case ImageQuality.Large:
                    return 2;
                case ImageQuality.Maximum:
                    return 3;
            }
            throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (ImageQuality)(int)value;
        }
    }
}
