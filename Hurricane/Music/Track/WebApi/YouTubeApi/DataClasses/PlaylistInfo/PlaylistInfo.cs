using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls.Dialogs;

namespace Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses.PlaylistInfo
{
    public class PageInfo
    {
        public int totalResults { get; set; }
        public int resultsPerPage { get; set; }
    }

    public class Default
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Medium
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class High
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Standard
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Maxres
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Thumbnails
    {
        public Default @default { get; set; }
        public Medium medium { get; set; }
        public High high { get; set; }
        public Standard standard { get; set; }
        public Maxres maxres { get; set; }
    }

    public class ResourceId
    {
        public string kind { get; set; }
        public string videoId { get; set; }
    }

    public class Snippet
    {
        public string publishedAt { get; set; }
        public string channelId { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Thumbnails thumbnails { get; set; }
        public string channelTitle { get; set; }
        public string playlistId { get; set; }
        public int position { get; set; }
        public ResourceId resourceId { get; set; }
    }

    public class Item : IVideoInfo
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public Snippet snippet { get; set; }
        public ContentDetails contentDetails { get; set; }

        public string title => snippet.title;
        public string uploader => snippet.channelTitle;
    }

    public class ContentDetails
    {
        public string videoId { get; set; }
    }

    public class PlaylistInfo : IPlaylistResult
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string nextPageToken { get; set; }
        public PageInfo pageInfo { get; set; }
        public List<Item> items { get; set; }

        public string Title { get; } = "Playlist";
        public string Uploader { get; }
        public BitmapImage Thumbnail { get; set; }
        public int TotalTracks => pageInfo.totalResults;
        public string PlaylistId { get; set; }

        public async Task<List<PlayableBase>> GetTracks(ProgressDialogController controller)
        {
            var currentPlaylist = this;
            var resultList = new List<PlayableBase>();

            for (int i = 0; i < (int)Math.Ceiling((double)TotalTracks / 50); i++)
            {
                var tracks = YouTubeApi.GetPlaylistTracks(await YouTubeApi.GetPlaylist(PlaylistId, currentPlaylist.nextPageToken, 50));
                for (int j = 0; j < tracks.Count; j++)
                {
                    var track = tracks[j];
                    if (LoadingTracksProcessChanged != null)
                        LoadingTracksProcessChanged(this, new LoadingTracksEventArgs(i * 50 + j, TotalTracks, track.Title));
                    resultList.Add(track.ToPlayable());
                    if (controller.IsCanceled) return null;
                }
            }

            return resultList;
        }

        public async Task LoadImage()
        {
            var url = items.First().snippet.thumbnails.@default.url;
            if (string.IsNullOrEmpty(url)) return;
            using (var client = new WebClient { Proxy = null })
            {
                Thumbnail = await Utilities.ImageHelper.DownloadImage(client, url);
            }
        }

        public event EventHandler<LoadingTracksEventArgs> LoadingTracksProcessChanged;
    }
}
