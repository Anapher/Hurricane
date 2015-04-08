using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Hurricane.Notification;

namespace Hurricane.Settings.Converter
{
    class NotificationTypeConverter : IValueConverter
    {
        private static readonly Dictionary<int, NotificationType> IndexValueDictionary = new Dictionary<int, NotificationType>()
        {
            {0, NotificationType.None},
            {1, NotificationType.Top},
            {2, NotificationType.RightBottom}
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IndexValueDictionary.First(x => x.Value == (NotificationType)value).Key;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IndexValueDictionary[(int)value];
        }
    }
}
