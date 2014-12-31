using System;
using System.Globalization;
using System.Windows.Data;
using Hurricane.Notification;

namespace Hurricane.Settings.Converter
{
    class NotificationTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((NotificationType)value)
            {
                case NotificationType.None:
                    return 0;
                case NotificationType.Top:
                    return 1;
                case NotificationType.RightBottom:
                    return 2;
            }
            throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((int)value)
            {
                case 0:
                    return NotificationType.None;
                case 1:
                    return NotificationType.Top;
                case 2:
                    return NotificationType.RightBottom;
            }
            throw new ArgumentException();
        }
    }
}
