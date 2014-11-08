using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Hurricane.Extensions.Converter
{
    class CurrentStateDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CSCore.SoundOut.PlaybackState state = (CSCore.SoundOut.PlaybackState)value;
            string imagepath;
            switch (state)
            {
                case CSCore.SoundOut.PlaybackState.Playing:
                    imagepath = "/Resources/MediaIcons/ThumbButtons/play.png";
                    break;
                case CSCore.SoundOut.PlaybackState.Paused:
                    imagepath = "/Resources/MediaIcons/ThumbButtons/pause.png";
                    break;
                default:
                    return null;
            }
            return imagepath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
