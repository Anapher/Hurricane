using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hurricane.Settings;
using Hurricane.Utilities;
using Newtonsoft.Json;

namespace Hurricane.Music.Track.YouTubeApi
{
    public class YouTubeApi
    {
        public static async Task<List<YouTubeWebTrackResult>> Search(string searchText)
        {
            using (var web = new WebClient { Proxy = null })
            {
                var link = string.Format("https://gdata.youtube.com/feeds/api/videos?q={0}&alt=json&max-results=50",
                    GeneralHelper.EscapeTitleName(searchText));
                var resultstr = await web.DownloadStringTaskAsync(link);
                var result = JsonConvert.DeserializeObject<ApiResult>(resultstr);
                if (result.feed.entry.Count == 0) return new List<YouTubeWebTrackResult>();
                return result.feed.entry.Select(x => new YouTubeWebTrackResult
                {
                    Duration = TimeSpan.FromSeconds(int.Parse(x.MediaGroup.Duration.seconds)),
                    Title = x.title.Name,
                    Uploader = x.author.First().name.Text,
                    Result = x,
                    ReleaseYear = (uint)DateTime.Parse(x.published.Date).Year,
                    ImageUrl = x.MediaGroup.Thumbnails.First().url,
                    Views = x.Statistics != null ? int.Parse(x.Statistics.viewCount) : 0
                }).ToList();
            }
        }

        public async static Task<BitmapImage> LoadBitmapImage(YouTubeTrack track, DirectoryInfo albumDirectory)
        {
            var config = HurricaneSettings.Instance.Config;

            using (var client = new WebClient { Proxy = null })
            {
                var image = await ImageHelper.DownloadImage(client, track.ThumbnailUrl);
                if (config.SaveCoverLocal)
                {
                    if (!albumDirectory.Exists) albumDirectory.Create();
                    await ImageHelper.SaveImage(image, track.YouTubeId, albumDirectory.FullName);
                }
                return image;
            }
        }
    }
}