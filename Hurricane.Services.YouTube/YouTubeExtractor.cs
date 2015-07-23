using System;
using System.Linq;
using System.Threading.Tasks;
using Hurricane.Model.AudioEngine;
using Hurricane.Services.YouTube.Extractor;

namespace Hurricane.Services.YouTube
{
   static class YouTubeExtractor
    {
       public static async Task<IPlaySource> GetPlaySource(string videoId)
       {
           var videoInfos =
               (await
                   Task.Run(
                       () => DownloadUrlResolver.GetDownloadUrls($"https://www.youtube.com/watch?v={videoId}", false)))
                   .Where(x => x.AudioType == AudioType.Aac || x.AudioType == AudioType.Mp3)
                   .OrderByDescending(info => info.AudioBitrate)
                   .ThenBy(x => x.Resolution);

           var video = videoInfos.First();

           if (video.RequiresDecryption)
               DownloadUrlResolver.DecryptDownloadUrl(video);

           return new HttpPlaySource(new Uri(video.DownloadUrl)) {Bitrate = video.AudioBitrate};
       }
    }
}
