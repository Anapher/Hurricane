using System;
using System.Linq;
using System.Windows.Data;
using Hurricane.Designer.Data;

namespace Hurricane.Designer.Converter
{
    public class ColorFromThemeDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var colorTheme = value as DataThemeBase;
            if (colorTheme == null) return null;
            return colorTheme.ThemeSettings.OfType<ThemeColor>().First(x => x.ID == parameter.ToString()).Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}