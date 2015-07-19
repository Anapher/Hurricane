namespace Hurricane.Model.Services
{
    public interface IDownloadable
    {
        /// <summary>
        /// The recommended filename of the downloaded file (without extension!)
        /// </summary>
        string RecommendedFilename { get; }

        /// <summary>
        /// The download method of the track
        /// </summary>
        IDownloadMethod Downloader { get; }

        /// <summary>
        /// The available extensions
        /// </summary>
        string[] Extensions { get; }
    }
}