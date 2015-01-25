namespace Hurricane.Music.Download
{
    class DownloadProgress
    {
        public double Progress { get; set; }
        public string MegaBytesDownloaded { get; set; }
        public string TotalBytesToDownload { get; set; }
        public string TimeLeft { get; set; }
        public string DownloadSpeed { get; set; }
    }
}