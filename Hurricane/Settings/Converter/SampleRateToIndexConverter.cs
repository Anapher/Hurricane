using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Hurricane.Settings.Converter
{
    class SampleRateToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch (int.Parse(value.ToString()))
            {
                case -1:
                    return 0; 
                case 44100:
                    return 1; 
                case 48000:
                    return 2; 
                case 88200:
                    return 3; 
                case 96000:
                    return 4; 
                case 176400:
                    return 5; 
                case 192000:
                    return 6; 
            }
            throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch (int.Parse(value.ToString()))
            {
                case 0:
                    return -1; 
                case 1:
                    return 44100; 
                case 2:
                    return 48000; 
                case 3:
                    return 88200; 
                case 4:
                    return 96000; 
                case 5:
                    return 96000; 
                case 6:
                    return 176400; 
                case 7:
                    return 192000; 
            }
            throw new ArgumentException();
        }
    }
}
