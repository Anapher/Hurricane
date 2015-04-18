using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hurricane.Settings;
using Hurricane.Utilities;

namespace Hurricane.Music.Track.WebApi.GroovesharkApi
{
    class GroovesharkApi : IMusicApi
    {
        public async Task<Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>> CheckForSpecialUrl(string url)
        {
            return new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(false, null, null);
        }

        public string ServiceName
        {
            get { return "Grooveshark"; }
        }

        public async Task<List<WebTrackResultBase>> Search(string searchText)
        {
            using (var wc = new WebClient { Proxy = null })
            {
                var result = JsonConvert.DeserializeObject<List<SearchResult>>(
                    await
                        wc.DownloadStringTaskAsync(
                            new Uri(string.Format("http://tinysong.com/s/{0}?format=json&limit=32&key={1}",
                                searchText.ToEscapedUrl(),
                                SensitiveInformation.TinySongKey))));
                if (result == null || result.Count == 0) return new List<WebTrackResultBase>();

                return result.Select(x => new GroovesharkWebTrackResult
                {
                    Duration = TimeSpan.Zero,
                    Album = x.AlbumName,
                    Uploader = x.ArtistName,
                    Url = x.Url,
                    Title = x.SongName,
                    SearchResult = x
                }).Cast<WebTrackResultBase>().ToList();
            }
        }

        public override string ToString()
        {
            return ServiceName;
        }

        public bool IsEnabled
        {
            get { return false; }
        }

        public System.Windows.FrameworkElement ApiSettings
        {
            get { return null; }
        }
    }
}