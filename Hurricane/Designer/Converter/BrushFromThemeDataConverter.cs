using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using Hurricane.Designer.Data;

namespace Hurricane.Designer.Converter
{
   public class BrushFromThemeDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colorTheme = value as DataThemeBase;
            if (colorTheme == null) return null;
            return new SolidColorBrush(colorTheme.ThemeSettings.OfType<ThemeColor>().First(x => x.ID == parameter.ToString()).Color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}