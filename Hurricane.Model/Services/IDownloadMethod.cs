using System;
using System.Threading.Tasks;

namespace Hurricane.Model.Services
{
    public interface IDownloadMethod
    {
        event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;
        Task Download(string path);
    }
}