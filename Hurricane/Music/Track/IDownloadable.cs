using Hurricane.Music.Download;

namespace Hurricane.Music.Track
{
    public interface IDownloadable
    {
        string DownloadParameter { get; }
        string DownloadFilename { get; }
        DownloadMethod DownloadMethod { get; }
        bool CanDownload { get; }
    }
}
