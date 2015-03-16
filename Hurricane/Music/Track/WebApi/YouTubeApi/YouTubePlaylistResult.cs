using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;

namespace Hurricane.Music.Track.WebApi.YouTubeApi
{
    public class YouTubePlaylistResult
    {
        public Feed feed { get; set; }

        public class Feed : IPlaylistResult
        {
            public string xmlns { get; set; }
            public Title title { get; set; }
            public List<Link> link { get; set; }
            public List<Author> author { get; set; }
            [JsonProperty("yt$playlistId")]
            public YtPlaylistId PlaylistId { get; set; }

            [JsonProperty("openSearch$totalResults")]
            public OpenSearchTotalResults SearchTotalResults { get; set; }
            [JsonProperty("openSearch$startIndex")]
            public OpenSearchStartIndex SearchStartIndex { get; set; }
            [JsonProperty("openSearch$itemsPerPage")]
            public OpenSearchItemsPerPage SearchItemsPerPage { get; set; }
            [JsonProperty("media$group")]
            public MediaGroup MediaGroup { get; set; }

            public List<Entry> entry { get; set; }

            public string Title
            {
                get { return title.Name; }
            }

            public BitmapImage Thumbnail { get; private set; }

            public int TotalTracks
            {
                get { return SearchTotalResults.Number; }
            }

            public async Task<List<PlayableBase>> GetTracks(ProgressDialogController controller)
            {
                var alltracks = SearchTotalResults.Number;
                
                var resultList = new List<PlayableBase>();
                var counter = 0;
                for (int i = 0; i < (int)Math.Ceiling((double)alltracks / 50); i++)
                {
                    var tracks = YouTubeApi.GetPlaylistTracks(await YouTubeApi.GetPlaylist(PlaylistId.Text, counter, 50));
                    for (int j = 0; j < tracks.Count; j++)
                    {
                        var track = tracks[j];
                        if (LoadingTracksProcessChanged != null)
                            LoadingTracksProcessChanged(this, new LoadingTracksEventArgs(counter + j, alltracks, track.Title));
                        resultList.Add(track.ToPlayable());
                        if (controller.IsCanceled) return null;
                    }
                    counter += 50;
                }
                return resultList;
            }

            public async Task LoadImage()
            {
                if (MediaGroup.Thumbnails == null || MediaGroup.Thumbnails.Count == 0) return;
                var url = MediaGroup.Thumbnails.First(x => x.url.EndsWith("mqdefault.jpg")).url;
                if (string.IsNullOrEmpty(url)) return;
                using (var client = new WebClient { Proxy = null })
                {
                    Thumbnail = await Utilities.ImageHelper.DownloadImage(client, url);
                }
            }

            public event EventHandler<LoadingTracksEventArgs> LoadingTracksProcessChanged;

            public string Uploader
            {
                get { return author != null && author.Count > 0 ? author.First().name.Text : string.Empty; }
            }
        }
    }
}
