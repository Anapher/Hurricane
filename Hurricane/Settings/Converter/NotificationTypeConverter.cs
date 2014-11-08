using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Hurricane.Settings.Converter
{
    class NotificationTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((Notification.NotificationType)value)
            {
                case Notification.NotificationType.None:
                    return 0;
                case Notification.NotificationType.Top:
                    return 1;
                case Notification.NotificationType.RightBottom:
                    return 2;
            }
            throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((int)value)
            {
                case 0:
                    return Notification.NotificationType.None;
                case 1:
                    return Notification.NotificationType.Top;
                case 2:
                    return Notification.NotificationType.RightBottom;
            }
            throw new ArgumentException();
        }
    }
}
