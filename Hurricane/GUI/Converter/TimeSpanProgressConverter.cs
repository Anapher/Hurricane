using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hurricane.GUI.Converter
{
    class TimeSpanProgressConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue) return 0;
            var newValue = (TimeSpan)values[0];
            var totallength = (TimeSpan)values[1];
            return newValue.TotalSeconds / totallength.TotalSeconds;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
