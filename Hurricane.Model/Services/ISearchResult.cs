using System;
using System.Windows.Media;
using Hurricane.Model.Music.Imagment;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Services
{
    public interface ISearchResult : IPlayable
    {
        new string Title { get; set; }
        new string Artist { get; set; }
        new ImageProvider Cover { get; set; }

        TimeSpan Duration { get; }
        Geometry ProviderIcon { get; }
        string ProviderName { get; }
        string Url { get; }

        ConversionInformation ConvertToStreamable();
    }

    public class ConversionInformation
    {
        public ConversionInformation(Streamable @base, string artist, string album)
        {
            Base = @base;
            Artist = artist;
            Album = album;
        }

        public Streamable Base { get; }
        public string Artist { get; set; }
        public string Album { get; set; }
    }
}