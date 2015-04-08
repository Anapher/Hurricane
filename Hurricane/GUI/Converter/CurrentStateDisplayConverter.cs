using System;
using System.Globalization;
using System.Windows.Data;
using CSCore.SoundOut;

namespace Hurricane.GUI.Converter
{
    class CurrentStateDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PlaybackState state = (PlaybackState)value;
            string imagepath;
            switch (state)
            {
                case PlaybackState.Playing:
                    imagepath = "/Resources/MediaIcons/ThumbButtons/play.png";
                    break;
                case PlaybackState.Paused:
                    imagepath = "/Resources/MediaIcons/ThumbButtons/pause.png";
                    break;
                default:
                    return null;
            }
            return imagepath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
