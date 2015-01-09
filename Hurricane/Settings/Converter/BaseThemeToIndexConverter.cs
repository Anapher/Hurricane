using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Hurricane.Settings.Themes;

namespace Hurricane.Settings.Converter
{
    class BaseThemeToIndexConverter : IValueConverter
    {
        private static readonly Dictionary<int, BaseTheme> indexValueDictionary = new Dictionary<int, BaseTheme>()
        {
            {0, BaseTheme.Light},
            {1, BaseTheme.Dark}
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return indexValueDictionary.First(x => x.Value == (BaseTheme) value).Key;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return indexValueDictionary[(int) value];
        }
    }
}
