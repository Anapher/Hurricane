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

namespace Hurricane.Music.Track.WebApi.YouTubeApi
{
    public class YouTubeApi : IMusicApi
    {
        public static async Task<BitmapImage> LoadBitmapImage(YouTubeTrack track, DirectoryInfo albumDirectory)
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

        public static List<YouTubeWebTrackResult> GetPlaylistTracks(YouTubePlaylistResult playlist)
        {
            if (playlist.feed != null && playlist.feed.entry != null && playlist.feed.entry.Count > 0)
            {
                return playlist.feed.entry.Where(x => x.MediaGroup.Duration != null).Select(x => new YouTubeWebTrackResult
                {
                    Duration = TimeSpan.FromSeconds(int.Parse(x.MediaGroup.Duration.seconds)),
                    Title = x.title.Name,
                    Uploader = x.author.First().name.Text,
                    Result = x,
                    Year = (uint)DateTime.Parse(x.published.Date).Year,
                    ImageUrl = x.MediaGroup.Thumbnails.First().url,
                    Views = x.Statistics != null ? uint.Parse(x.Statistics.viewCount) : 0,
                    Url = x.link.First().href,
                    Description = x.MediaGroup.Description.Text
                }).ToList();

/*                List<YouTubeWebTrackResult> lst = new List<YouTubeWebTrackResult>();
                foreach (var entry in playlist.feed.entry)
                {
                    if (entry.title.Name == "Private video" || entry.MediaGroup.Duration == null) continue;
                    var yt = new YouTubeWebTrackResult();
                    if (entry.MediaGroup == null || entry.MediaGroup.Duration == null ||
                        entry.MediaGroup.Duration.seconds == null)
                    {
                        Debug.Print("hey");
                    }
                    yt.Duration = TimeSpan.FromSeconds(int.Parse(entry.MediaGroup.Duration.seconds)); //
                    yt.Title = entry.title.Name;
                    yt.Uploader = entry.author.First().name.Text;
                    yt.Result = entry;
                    yt.Year = (uint) DateTime.Parse(entry.published.Date).Year;
                    yt.ImageUrl = entry.MediaGroup.Thumbnails.First().url;
                    yt.Views = entry.Statistics != null ? int.Parse(entry.Statistics.viewCount) : 0;
                    yt.Url = entry.link.First().href;
                    lst.Add(yt);
                }*/ //if we have to find an error
            }
            return null;
        }

        public static async Task<YouTubePlaylistResult> GetPlaylist(string playlistId, int index, int maxResults)
        {
            using (var web = new WebClient { Proxy = null })
            {
                var link = string.Format("https://gdata.youtube.com/feeds/api/playlists/{0}?max-results={1}{2}&alt=json", playlistId, maxResults, index > 0 ? "&start-index=" + index : string.Empty);
                var resultstr = await web.DownloadStringTaskAsync(link);
                var result = JsonConvert.DeserializeObject<YouTubePlaylistResult>(resultstr);
                return result;
            }
        }

        private static async Task<YouTubeWebTrackResult> GetYouTubeVideoInfo(string youtubeVideoId)
        {
            using (var web = new WebClient { Proxy = null })
            {
                var link = string.Format("https://gdata.youtube.com/feeds/api/videos/{0}?v=2&alt=json", youtubeVideoId);
                var resultstr = await web.DownloadStringTaskAsync(link);
                var result = JsonConvert.DeserializeObject<YouTubeVideoResult>(resultstr);
                if (result.entry != null)
                {
                    return new YouTubeWebTrackResult
                    {
                        Duration = TimeSpan.FromSeconds(int.Parse(result.entry.MediaGroup.Duration.seconds)),
                        Title = result.entry.title.Name,
                        Uploader = result.entry.author.First().name.Text,
                        Result = result.entry,
                        Year = (uint)DateTime.Parse(result.entry.published.Date).Year,
                        ImageUrl = result.entry.MediaGroup.Thumbnails.First().url,
                        Views = result.entry.Statistics != null ? uint.Parse(result.entry.Statistics.viewCount) : 0,
                        Url = result.entry.link.First().href,
                        Description = result.entry.MediaGroup.Description.Text
                    };
                }
                return null;
            }
        }

        public async Task<Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>> CheckForSpecialUrl(string url)
        {
            var match = Regex.Match(url, @"youtu(?:\.be|be\.com).*?[&?]list=(?<id>[a-zA-Z0-9-_]+)");
            if (match.Success)
            {
                var playlist = await GetPlaylist(match.Groups["id"].Value, 0, 50);
                await playlist.feed.LoadImage();
                return new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(true, GetPlaylistTracks(playlist).Cast<WebTrackResultBase>().ToList(), playlist.feed);
            }

            match = Regex.Match(url, @"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)(?<id>[a-zA-Z0-9-_]+)");
            if (match.Success)
            {
                return new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(true, new List<WebTrackResultBase> { await GetYouTubeVideoInfo(match.Groups["id"].Value) }, null);
            }
            return new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(false, null, null);
        }

        public string ServiceName
        {
            get { return "YouTube"; }
        }

        public async Task<List<WebTrackResultBase>> Search(string searchText)
        {
            using (var web = new WebClient { Proxy = null })
            {
                var link = string.Format("https://gdata.youtube.com/feeds/api/videos?q={0}&alt=json&max-results=50",
                    searchText.ToEscapedUrl());
                var result = JsonConvert.DeserializeObject<YouTubeSearchResult>(await web.DownloadStringTaskAsync(link));
                if (result.feed == null || result.feed.entry == null || result.feed.entry.Count == 0) return new List<WebTrackResultBase>();
                return result.feed.entry.Where(x => x.MediaGroup.Duration != null).Select(x => new YouTubeWebTrackResult
                {
                    Duration = TimeSpan.FromSeconds(int.Parse(x.MediaGroup.Duration.seconds)),
                    Title = x.title.Name,
                    Uploader = x.author.First().name.Text,
                    Result = x,
                    Year = (uint)DateTime.Parse(x.published.Date).Year,
                    ImageUrl = x.MediaGroup.Thumbnails.First().url,
                    Views = x.Statistics != null ? uint.Parse(x.Statistics.viewCount) : 0,
                    Url = x.link.First().href,
                    Description = x.MediaGroup.Description.Text
                }).Cast<WebTrackResultBase>().ToList();
            }
        }

        public override string ToString()
        {
            return ServiceName;
        }

        public bool IsEnabled
        {
            get { return true; }
        }

        public System.Windows.FrameworkElement ApiSettings
        {
            get { return null; }
        }
    }
}