using System.Net;
using System.Threading.Tasks;
using Hurricane.Settings;

namespace Hurricane.Music.Download
{
    class SoundCloudDownloader
    {
        public static async Task DownloadSoundCloudTrack(string SoundCloudID, DownloadEntry entry)
        {
            using (var client = new WebClient {Proxy = null})
            {
                client.DownloadProgressChanged += (s, e) =>  entry.Progress = e.ProgressPercentage;
                await client.DownloadFileTaskAsync(string.Format("https://api.soundcloud.com/tracks/{0}/download?client_id={1}", SoundCloudID, SensitiveInformation.SoundCloudKey), entry.Filename);
            }
            entry.IsDownloaded = true;
        }
    }
}
