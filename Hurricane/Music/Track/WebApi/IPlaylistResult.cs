using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Hurricane.Music.Track.WebApi
{
    public interface IPlaylistResult
    {
        string Title { get; }
        string Uploader { get; }
        BitmapImage Thumbnail { get; }
        int TotalTracks { get; }
        Task<List<PlayableBase>> GetTracks();
        Task LoadImage();
        event EventHandler<LoadingTracksEventArgs> LoadingTracksProcessChanged;
    }
}