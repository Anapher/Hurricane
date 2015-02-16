using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using MahApps.Metro.Controls;

namespace Hurricane.Settings.Converter
{
    public class TransitionConverter : IValueConverter
    {
        private static readonly Dictionary<int, TransitionType> indexValueDictionary = new Dictionary<int, TransitionType>()
        {
            {0, TransitionType.Left},
            {1, TransitionType.Right},
            {2, TransitionType.Up},
            {3, TransitionType.Down},
            {4, TransitionType.Default},
            {6, TransitionType.Normal}
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return indexValueDictionary.First(x => x.Value == (TransitionType)value).Key;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return indexValueDictionary[(int)value];
        }
    }
}
