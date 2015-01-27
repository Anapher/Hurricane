using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hurricane.Settings;
using Newtonsoft.Json;

namespace Hurricane.Music.Track.WebApi.SoundCloudApi
{
    class SoundCloudApi : IMusicApi
    {
                public static async Task<BitmapImage> LoadBitmapImage(SoundCloudTrack track, ImageQuality quality, DirectoryInfo albumDirectory)
        {
            var config = HurricaneSettings.Instance.Config;

            using (var client = new WebClient { Proxy = null })
            {
                var image = await Utilities.ImageHelper.DownloadImage(client, string.Format(track.ArtworkUrl, GetQualityModifier(quality)));
                if (config.SaveCoverLocal)
                {
                    if (!albumDirectory.Exists) albumDirectory.Create();
                    await Utilities.ImageHelper.SaveImage(image, string.Format("{0}_{1}.jpg", track.SoundCloudID, GetQualityModifier(quality)), albumDirectory.FullName);
                }
                return image;
            }
        }

        public static string GetQualityModifier(ImageQuality quality)
        {
            switch (quality)
            {
                case ImageQuality.Small:
                    return "large";  //100x100
                case ImageQuality.Medium:
                    return "t300x300"; //300x300
                case ImageQuality.Large:
                    return "crop"; //400x400
                case ImageQuality.Maximum:
                    return "t500x500"; //500x500
                default:
                    throw new ArgumentOutOfRangeException("quality");
            }
        }

        private async Task<SoundCloudWebTrackResult> GetSoundCloudTrack(string url)
        {
            using (var web = new WebClient { Proxy = null })
            {
                try
                {
                    var result = JsonConvert.DeserializeObject<ApiResult>(await web.DownloadStringTaskAsync(string.Format("http://api.soundcloud.com/resolve.json?url={0}&client_id={1}", url, SensitiveInformation.SoundCloudKey)));
                    return new SoundCloudWebTrackResult
                    {
                        Duration = TimeSpan.FromMilliseconds(result.duration),
                        Year = result.release_year != null ? uint.Parse(result.release_year.ToString()) : (uint)DateTime.Parse(result.created_at).Year,
                        Title = result.title,
                        Uploader = result.user.username,
                        Result = result,
                        Views = (uint)result.playback_count,
                        ImageUrl = result.artwork_url,
                        Url = result.permalink_url,
                        Genres = result.genre
                    };
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        async Task<Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>> IMusicApi.CheckForSpecialUrl(string url)
        {
            //http://api.soundcloud.com/resolve.json?url=http://soundcloud.com/matas/hobnotropic&client_id=YOUR_CLIENT_ID
            var match = Regex.Match(url, @"soundcloud.com\/.*\/.*");
            if (match.Success)
            {
                var track = await GetSoundCloudTrack(url);
                if (track != null)
                {
                    return new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(true,
                        new List<WebTrackResultBase> { track }, null);
                }
            }

            return new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(false, null, null);
        }

        public string ServiceName
        {
            get { return "SoundCloud"; }
        }

        async Task<List<WebTrackResultBase>> IMusicApi.Search(string searchText)
        {
            using (var web = new WebClient { Proxy = null })
            {
                var results = JsonConvert.DeserializeObject<List<ApiResult>>(await web.DownloadStringTaskAsync(string.Format("https://api.soundcloud.com/tracks?q={0}&client_id={1}", Utilities.GeneralHelper.EscapeTitleName(searchText), SensitiveInformation.SoundCloudKey)));
                return results.Where(x => x.streamable).Select(x => new SoundCloudWebTrackResult
                {
                    Duration = TimeSpan.FromMilliseconds(x.duration),
                    Year = x.release_year != null ? uint.Parse(x.release_year.ToString()) : (uint)DateTime.Parse(x.created_at).Year,
                    Title = x.title,
                    Uploader = x.user.username,
                    Result = x,
                    Views = (uint)x.playback_count,
                    ImageUrl = x.artwork_url,
                    Url = x.permalink_url,
                    Genres = x.genre
                }).Cast<WebTrackResultBase>().ToList();
            }
        }

        public override string ToString()
        {
            return ServiceName;
        }
    }
}
