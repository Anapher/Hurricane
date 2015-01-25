using Hurricane.Music.Track;
using Hurricane.ViewModelBase;

namespace Hurricane.Music.Download
{
    public class DownloadEntry : PropertyChangedBase, IDownloadable
    {
        private bool _isWaiting;
        public bool IsWaiting
        {
            get { return _isWaiting; }
            set
            {
                SetProperty(value, ref _isWaiting);
            }
        }
        
        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set
            {
                SetProperty(value, ref _progress);
            }
        }
        
        private bool _isDownloaded;
        public bool IsDownloaded
        {
            get { return _isDownloaded; }
            set
            {
                SetProperty(value, ref _isDownloaded);
            }
        }

        public string Trackname { get; set; }
        public DownloadMethod DownloadMethod { get; set; }
        public string DownloadParameter { get; set; }
        public IMusicInformation MusicInformation { get; set; }
        public string DownloadFilename { get; set; }

        public bool CanDownload
        {
            get { return true; }
        }
    }

    public enum DownloadMethod { SoundCloud, youtube_dl }
}
