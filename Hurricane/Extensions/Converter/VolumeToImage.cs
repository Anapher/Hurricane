using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Hurricane.Extensions.Converter
{
    class VolumeToImage : IValueConverter
    {
        private static BitmapImage _muteimage;
        private static BitmapImage _muteimageLight;

        private static BitmapImage _mediumimage;
        private static BitmapImage _mediumimageLight;

        private static BitmapImage _loudimage;
        private static BitmapImage _loudimageLight;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool light = (bool)Application.Current.Resources["LightVolumeIcon"];
            double volume = (double)value;
            if (volume == 0)
            {
                return light ? _muteimageLight ?? (_muteimageLight = new BitmapImage(new Uri(@"/Resources/MediaIcons/Advanced/Volume/mute_light.png", UriKind.Relative))) : _muteimage ?? (_muteimage = new BitmapImage(new Uri(@"/Resources/MediaIcons/Advanced/Volume/mute.png", UriKind.Relative)));
            }
            if (volume <= 0.5)
            {
                return light ? _mediumimageLight ?? (_mediumimageLight = new BitmapImage(new Uri(@"/Resources/MediaIcons/Advanced/Volume/medium_light.png", UriKind.Relative))) : _mediumimage ?? (_mediumimage = new BitmapImage(new Uri(@"/Resources/MediaIcons/Advanced/Volume/medium.png", UriKind.Relative)));
            }
            return light ? _loudimageLight ?? (_loudimageLight = new BitmapImage(new Uri(@"/Resources/MediaIcons/Advanced/Volume/loud_light.png", UriKind.Relative))) : _loudimage ?? (_loudimage = new BitmapImage(new Uri(@"/Resources/MediaIcons/Advanced/Volume/loud.png", UriKind.Relative)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
