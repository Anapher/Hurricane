using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Hurricane.Music.Playlist;

namespace Hurricane.GUI.Converter
{
    public class PlaylistInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var playlist = (IPlaylist)value;
            if (playlist == null) return null;
            switch (parameter.ToString())
            {
                case "alltrackscount":
                    return playlist.Tracks.Count;
                case "alltracksduration":
                    var allDuration = TimeSpan.Zero;
                    allDuration = playlist.Tracks.Aggregate(allDuration, (current, track) => current + track.DurationTimespan);
                    return allDuration.ToString(@"hh\:mm\:ss");
                default:
                    throw new ArgumentException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}