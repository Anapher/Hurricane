using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls.Dialogs;

namespace Hurricane.Music.Track.WebApi
{
    public interface IPlaylistResult
    {
        string Title { get; }
        string Uploader { get; }
        BitmapImage Thumbnail { get; }
        int TotalTracks { get; }
        Task<List<PlayableBase>> GetTracks(ProgressDialogController controller);
        Task LoadImage();
        event EventHandler<LoadingTracksEventArgs> LoadingTracksProcessChanged;
    }
}