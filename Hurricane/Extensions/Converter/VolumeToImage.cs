using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Hurricane.Extensions.Converter
{
    class VolumeToImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double volume = (double)value;
            if (volume == 0)
            {
                return new BitmapImage(new Uri(@"/Resources/MediaIcons/Advanced/Volume/mute.png", UriKind.Relative));
            }
            if (volume <= 0.5)
            {
                return new BitmapImage(new Uri(@"/Resources/MediaIcons/Advanced/Volume/medium.png", UriKind.Relative));
            }
            return new BitmapImage(new Uri(@"/Resources/MediaIcons/Advanced/Volume/loud.png", UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
