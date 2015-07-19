using System;
using System.Net;
using System.Threading.Tasks;

namespace Hurricane.Model.Services
{
    public class WebClientDownloadMethod : IDownloadMethod
    {
        private readonly string _downloadUrl;

        public WebClientDownloadMethod(string downloadUrl)
        {
            _downloadUrl = downloadUrl;
        }

        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

        public Task Download(string path)
        {
            using (var webClient = new WebClient {Proxy = null})
            {
                webClient.DownloadProgressChanged +=
                    (sender, args) =>
                        DownloadProgressChanged?.Invoke(this,
                            new DownloadProgressChangedEventArgs(args.BytesReceived, args.TotalBytesToReceive));

                return webClient.DownloadFileTaskAsync(_downloadUrl, path);
            }
        }
    }
}