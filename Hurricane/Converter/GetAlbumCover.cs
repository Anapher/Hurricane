using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.TrackProperties;

namespace Hurricane.Converter
{
    class GetAlbumCover : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var album = (Album)values[0];
            var tracks = (ObservableCollection<PlayableBase>) values[1];

            foreach (var track in tracks)
            {
                if (track.Album == album && track.Cover != null)
                    return track.Cover;
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
