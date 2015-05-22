using System;
using System.Globalization;
using System.Windows.Data;

namespace Hurricane.Converter
{
    class TimespanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeSpan = (TimeSpan)value;
            return timeSpan.Hours > 0 || timeSpan.Days > 0
                ? string.Format("{0:00}:{1:mm}:{1:ss}", (int) timeSpan.TotalHours, timeSpan)
                : timeSpan.ToString(@"mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
