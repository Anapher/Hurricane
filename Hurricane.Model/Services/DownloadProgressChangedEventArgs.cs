using System;

namespace Hurricane.Model.Services
{
    public class DownloadProgressChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadProgressChangedEventArgs"/> class.
        /// </summary>
        /// <param name="progress">The current progress (0 - 1)</param>
        public DownloadProgressChangedEventArgs(double progress)
        {
            Progress = progress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadProgressChangedEventArgs"/> class.
        /// </summary>
        /// <param name="progress">The current progress (0 - 1)</param>
        /// <param name="bytesReceived">The received bytes</param>
        /// <param name="totalBytes">Total bytes to receive</param>
        public DownloadProgressChangedEventArgs(double progress, long bytesReceived, long totalBytes)
        {
            Progress = progress;
            BytesReceived = bytesReceived;
            TotalBytes = totalBytes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadProgressChangedEventArgs"/> class. The progress becomes calculated from <see cref="bytesReceived"/> / <see cref="totalBytes"/>
        /// </summary>
        /// <param name="bytesReceived">The received bytes</param>
        /// <param name="totalBytes">otal bytes to receive</param>
        public DownloadProgressChangedEventArgs(long bytesReceived, long totalBytes)
        {
            BytesReceived = bytesReceived;
            TotalBytes = totalBytes;
            Progress = bytesReceived / (double)totalBytes;
        }

        /// <summary>
        /// The received bytes
        /// </summary>
        public long BytesReceived { get; }

        /// <summary>
        /// Total bytes to receive
        /// </summary>
        public long TotalBytes { get; }

        /// <summary>
        /// The current progress
        /// </summary>
        public double Progress { get; }
    }
}