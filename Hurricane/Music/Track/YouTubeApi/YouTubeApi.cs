using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
                var result = JsonConvert.DeserializeObject<YouTubeSearchResult>(resultstr);
                if (result.feed == null || result.feed.entry == null || result.feed.entry.Count == 0) return new List<YouTubeWebTrackResult>();
                return result.feed.entry.Select(x => new YouTubeWebTrackResult
                {
                    Duration = TimeSpan.FromSeconds(int.Parse(x.MediaGroup.Duration.seconds)),
                    Title = x.title.Name,
                    Uploader = x.author.First().name.Text,
                    Result = x,
                    Year = (uint)DateTime.Parse(x.published.Date).Year,
                    ImageUrl = x.MediaGroup.Thumbnails.First().url,
                    Views = x.Statistics != null ? int.Parse(x.Statistics.viewCount) : 0,
                    Url = x.link.First().href
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

        public async static Task<Tuple<bool, List<YouTubeWebTrackResult>>> CheckForSpecialUrl(string url)
        {
            var match = Regex.Match(url, @"youtu(?:\.be|be\.com).*?[&?]list=(.+)");
            if (match.Success)
            {
                using (var web = new WebClient { Proxy = null })
                {
                    var link = string.Format("https://gdata.youtube.com/feeds/api/playlists/{0}?max-results=50&alt=json", match.Groups[match.Groups.Count -1]);
                    var resultstr = await web.DownloadStringTaskAsync(link);
                    var result = JsonConvert.DeserializeObject<YouTubePlaylistResult>(resultstr);
                    if (result.feed != null && result.feed.entry != null && result.feed.entry.Count > 0)
                    {
                        return new Tuple<bool, List<YouTubeWebTrackResult>>(true,
                            result.feed.entry.Select(x => new YouTubeWebTrackResult
                            {
                                Duration = TimeSpan.FromSeconds(int.Parse(x.MediaGroup.Duration.seconds)),
                                Title = x.title.Name,
                                Uploader = x.author.First().name.Text,
                                Result = x,
                                Year = (uint) DateTime.Parse(x.published.Date).Year,
                                ImageUrl = x.MediaGroup.Thumbnails.First().url,
                                Views = x.Statistics != null ? int.Parse(x.Statistics.viewCount) : 0,
                                Url = x.link.First().href
                            }).ToList());
                    }
                    return new Tuple<bool, List<YouTubeWebTrackResult>>(false, null);
                }
            }

            match = Regex.Match(url, @"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)");
            if (match.Success)
            {
                using (var web = new WebClient { Proxy = null })
                {
                    var link = string.Format("https://gdata.youtube.com/feeds/api/videos/{0}?v=2&alt=json", match.Groups[match.Groups.Count - 1].Value);
                    var resultstr = await web.DownloadStringTaskAsync(link);
                    var result = JsonConvert.DeserializeObject<YouTubeVideoResult>(resultstr);
                    if (result.entry != null )
                    {
                        return new Tuple<bool, List<YouTubeWebTrackResult>>(true, new List<YouTubeWebTrackResult>
                        {
                            new YouTubeWebTrackResult
                            {
                                Duration = TimeSpan.FromSeconds(int.Parse(result.entry.MediaGroup.Duration.seconds)),
                                Title = result.entry.title.Name,
                                Uploader = result.entry.author.First().name.Text,
                                Result = result.entry,
                                Year = (uint) DateTime.Parse(result.entry.published.Date).Year,
                                ImageUrl = result.entry.MediaGroup.Thumbnails.First().url,
                                Views = result.entry.Statistics != null ? int.Parse(result.entry.Statistics.viewCount) : 0,
                                Url = result.entry.link.First().href
                            }
                        });
                    }
                }
            }
            return new Tuple<bool, List<YouTubeWebTrackResult>>(false, null);
        }
    }
}