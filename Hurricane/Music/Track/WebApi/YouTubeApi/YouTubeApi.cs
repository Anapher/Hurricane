using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml;
using Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses;
using Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses.PlaylistInfo;
using Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses.SearchResult;
using Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses.VideoInfo;
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

        public static List<YouTubeWebTrackResult> GetPlaylistTracks(PlaylistInfo playlist)
        {
            if (playlist?.items != null && playlist.items.Count > 0)
            {
                return playlist.items.Select(x => new YouTubeWebTrackResult
                {
                    Duration = TimeSpan.Zero,
                    Title = x.snippet.title,
                    Uploader = x.snippet.channelTitle,
                    Result = x,
                    Year = (uint)DateTime.Parse(x.snippet.publishedAt).Year,
                    ImageUrl = x.snippet.thumbnails.@default.url,
                    Views =  0,
                    Url = $"https://www.youtube.com/watch?v={x.contentDetails.videoId}",
                    Description = x.snippet.description
                }).ToList();
            }
            return null;
        }

        public static async Task<PlaylistInfo> GetPlaylist(string playlistId, string nextPageToken, int maxResults)
        {
            using (var web = new WebClient { Proxy = null })
            {
                var pageTokenPart = string.IsNullOrEmpty(nextPageToken) ? null : "&pageToken=" + nextPageToken; 
                var link =
                    $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet%2CcontentDetails&maxResults={maxResults}&playlistId={playlistId}{pageTokenPart}&key={SensitiveInformation.YouTubeApiKey}";

                var resultstr = await web.DownloadStringTaskAsync(link);
                var result = JsonConvert.DeserializeObject<PlaylistInfo>(resultstr);
                result.PlaylistId = playlistId;
                return result;
            }
        }

        private static async Task<YouTubeWebTrackResult> GetYouTubeVideoInfo(string youtubeVideoId)
        {
            using (var web = new WebClient { Proxy = null })
            {
                var link =
                    $"https://www.googleapis.com/youtube/v3/videos?id={youtubeVideoId}&part=snippet,contentDetails&key={SensitiveInformation.YouTubeApiKey}";
                var resultstr = await web.DownloadStringTaskAsync(link);
                var result = JsonConvert.DeserializeObject<GetVideoInfoResult>(resultstr);
                if (result?.items != null && result.items.Count > 0)
                {
                    var video = result.items[0];
                    return new YouTubeWebTrackResult
                    {
                        Duration = XmlConvert.ToTimeSpan(video.contentDetails.duration),
                        Title = video.snippet.title,
                        Uploader = video.snippet.channelTitle,
                        Result = video,
                        Year = (uint)DateTime.Parse(video.snippet.publishedAt).Year,
                        ImageUrl =video.snippet.thumbnails.@default.url,
                        Views = 0,
                        Url = $"https://www.youtube.com/watch?v={youtubeVideoId}",
                        Description = video.snippet.description
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
                var playlist = await GetPlaylist(match.Groups["id"].Value, null, 50);
                await playlist.LoadImage();
                return new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(true, GetPlaylistTracks(playlist).Cast<WebTrackResultBase>().ToList(), playlist);
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
                var link =
                    $"https://www.googleapis.com/youtube/v3/search?part=snippet&q={searchText.ToEscapedUrl()}&maxResults=50&key={SensitiveInformation.YouTubeApiKey}";
                var result = JsonConvert.DeserializeObject<SearchResult>(await web.DownloadStringTaskAsync(link));
                if (result?.items == null || result.items.Count == 0)
                    return new List<WebTrackResultBase>();

                var videos = result.items.Where(x => x.id.kind == "youtube#video").ToList();

                var detailedInfo =
                    JsonConvert.DeserializeObject<ContentSearchResult>(
                        await
                            web.DownloadStringTaskAsync(
                                $"https://www.googleapis.com/youtube/v3/videos?id={string.Join(",", videos.Select(x => x.id.videoId))}&part=contentDetails&key={SensitiveInformation.YouTubeApiKey}"));

                return videos.Select(x => new YouTubeWebTrackResult
                {
                    Duration = XmlConvert.ToTimeSpan(detailedInfo.items.First(y => y.id == x.id.videoId).contentDetails.duration),
                    Title = x.snippet.title,
                    Uploader = x.snippet.channelTitle,
                    Result = x,
                    Year = (uint)DateTime.Parse(x.snippet.publishedAt).Year,
                    ImageUrl = x.snippet.thumbnails.@default.url,
                    Views = 0,
                    Url = $"https://www.youtube.com/watch?v={x.id.videoId}",
                    Description = x.snippet.description
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