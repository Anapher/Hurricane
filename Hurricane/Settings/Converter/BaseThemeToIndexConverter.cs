using System;
using System.Globalization;
using System.Windows.Data;
using Hurricane.Settings.Themes;

namespace Hurricane.Settings.Converter
{
    class BaseThemeToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((BaseTheme) value)
            {
                case BaseTheme.Light:
                    return 0;
                case BaseTheme.Dark:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((int)value)
            {
                case 0:
                    return BaseTheme.Light;
                case 1:
                    return BaseTheme.Dark;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
