using System;
using System.Globalization;
using System.Windows.Data;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Converter
{
    [ValueConversion(typeof(PlayableBase), typeof(TrackType))]
    class GetTrackTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is LocalPlayable)
                return TrackType.Local;

            if (value is Streamable)
                return TrackType.Stream;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    enum TrackType
    {
        Stream,
        Local
    }
}
