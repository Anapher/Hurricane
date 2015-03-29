using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using Hurricane.Music.Download;

namespace Hurricane.Settings.Converter
{
    class AudioFormatToIndexConverter : IValueConverter
    {
        private static readonly Dictionary<int, AudioFormat> indexValueDictionary = new Dictionary<int, AudioFormat>()
        {
            {0, AudioFormat.Copy},
            {1, AudioFormat.MP3},
            {2, AudioFormat.AAC},
            {3, AudioFormat.WMA}
        };

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return indexValueDictionary.First(x => x.Value == (AudioFormat)value).Key;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return indexValueDictionary[(int)value];
        }
    }
}
