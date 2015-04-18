using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Hurricane.Music.Track
{
    public interface IMusicInformation
    {
        string Title { get; set; }
        TimeSpan Duration { get; set; }
        string Artist { get; set; }
        Task<BitmapImage> GetImage();
        List<Genre> Genres { get; set; }
        uint Year { get; set; }
        string Album { get; set; }
    }

    public enum Genre
    {
        Alternative,
        Pop,
        Rock,
        RapAndHipHop,
        Classical,
        DanceAndHouse,
        Instrumental,
        Metal,
        Dubstep,
        IndiePop,
        Speech,
        DrumAndBass,
        Trance,
        Chanson,
        Ethnic,
        AcousticAndVocal,
        Reggae,
        Country,
        EasyListening,
        JazzAndBlues,
        ElectropopAndDisco,
        SoundTrack,
        Other
    }
}