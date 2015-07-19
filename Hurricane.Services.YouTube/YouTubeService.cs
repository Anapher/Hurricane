using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using Hurricane.Model.Services;
using Hurricane.Services.YouTube.Data.ContentDetailsRequest;
using Hurricane.Services.YouTube.Data.SearchRequest;
using Newtonsoft.Json;

namespace Hurricane.Services.YouTube
{
    public class YouTubeService : IMusicStreamingService<YouTubeTrack>
    {
        private const string YouTubeApiKey = "AIzaSyAc9QR-wezOFwqV-tggNS7XUpTlMvEZekk";
        private static Geometry _youtubeVector;

        public string Name { get; } = "YouTube";
        public string Url { get; } = "https://www.youtube.com/";
        public Geometry Icon => GetYouTubeVector();
        public Type StreamableType { get; } = typeof (YouTubeTrack);

        public Task<IEnumerable<ISearchResult>> FastSearch(string query)
        {
            return EasySearch(query, 5);
        }

        public Task<IEnumerable<ISearchResult>> Search(string query)
        {
            return EasySearch(query, 50);
        }

        public async Task<ISearchResult> GetTrack(string trackName)
        {
            return (await EasySearch(trackName, 3)).OrderBy(x => x.Duration).First();
        }

        private async Task<IEnumerable<ISearchResult>> EasySearch(string query, uint maxResults)
        {
            using (var webClient = new WebClient { Proxy = null })
            {
                var result =
                    JsonConvert.DeserializeObject<SearchResult>(
                        await
                            webClient.DownloadStringTaskAsync(
                                $"https://www.googleapis.com/youtube/v3/search?order=relevance&type=video&regionCode={CultureInfo.CurrentCulture.TwoLetterISOLanguageName}&part=snippet&videoDuration=any&maxResults={maxResults}&key={YouTubeApiKey}&q={Uri.EscapeUriString(query)}"));

                var result2 =
                    JsonConvert.DeserializeObject<ContentDetailsResult>(
                        await
                            webClient.DownloadStringTaskAsync(
                                $"https://www.googleapis.com/youtube/v3/videos?id={string.Join(",", result.items.Select(x => x.id.videoId))}&part=contentDetails&key={YouTubeApiKey}"));

                return
                    result.items.Select(
                        x =>
                            new YouTubeSearchResult(x.snippet.title, GetArtist(x.snippet.title, x.snippet.channelTitle),
                                x.snippet.channelTitle, x.id.videoId, XmlConvert.ToTimeSpan(
                                    result2.items.First(y => y.id == x.id.videoId).contentDetails.duration),
                                x.snippet.thumbnails.@default.url));
            }
        }

        private string GetArtist(string title, string channelName)
        {
            var match = title.Split('-');
            if (match.Length > 0 && match[0].Length > 1)
            {
                return match[0];
            }
            return channelName;
        }

        public static Geometry GetYouTubeVector()
        {
            if (_youtubeVector == null)
            {
                //from https://www.youtube.com/yt/brand/de/downloads.html
                _youtubeVector =
                    Geometry.Parse(
                        "F1 M 405.272,491.704 L 405.225,204.704 L 681.225,348.704 L 405.272,491.704 Z M 1011.248,154.985 C 1011.248,154.985 1001.267,84.596 970.642,53.599 C 931.800,12.916 888.262,12.714 868.296,10.332 C 725.359,0.000 510.946,0.000 510.946,0.000 L 510.502,0.000 C 510.502,0.000 296.094,0.000 153.152,10.332 C 133.185,12.714 89.663,12.916 50.807,53.599 C 20.181,84.596 10.215,154.985 10.215,154.985 C 10.215,154.985 0.000,237.645 0.000,320.304 L 0.000,397.797 C 0.000,480.455 10.215,563.114 10.215,563.114 C 10.215,563.114 20.181,633.504 50.807,664.501 C 89.663,705.185 140.703,703.898 163.435,708.162 C 245.153,715.998 510.725,718.423 510.725,718.423 C 510.725,718.423 725.359,718.100 868.296,707.768 C 888.262,705.386 931.800,705.185 970.642,664.501 C 1001.267,633.504 1011.248,563.114 1011.248,563.114 C 1011.248,563.114 1021.449,480.455 1021.449,397.797 L 1021.449,320.304 C 1021.449,237.645 1011.248,154.985 1011.248,154.985 Z");
                _youtubeVector.Freeze();
            }

            return _youtubeVector;
        }
    }
}