using System;
using System.Globalization;
using System.Windows.Data;

namespace Hurricane.Settings.Converter
{
    class BitrateToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BitrateToIndex(int.Parse(value.ToString()));
        }

        public static int BitrateToIndex(int waveSourceBits)
        {
            switch (waveSourceBits)
            {
                case 8:
                    return 0;
                case 16:
                    return 1;
                case 24:
                    return 2;
                case 32:
                    return 3;
            }
            throw new ArgumentException();
        }

        public static int IndexToBitrate(int index)
        {
            switch (index)
            {
                case 0:
                   return 8;
                case 1:
                    return 16;
                case 2:
                    return 24;
                case 3:
                    return 32;
            }
            throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IndexToBitrate(int.Parse(value.ToString()));
        }
    }
}
