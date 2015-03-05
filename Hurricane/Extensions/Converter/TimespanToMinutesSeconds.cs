using System;
using System.Globalization;
using System.Windows.Data;

namespace Hurricane.Extensions.Converter
{
    class TimespanToMinutesSeconds : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "00:00";
            TimeSpan time = (TimeSpan)value;

            return string.Format("{0:00}:{1:ss}", (int)time.TotalMinutes, time);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
