using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hurricane.Settings;
using Newtonsoft.Json;

namespace Hurricane.Music.Track.WebApi.SoundCloudApi
{
    class SoundCloudApi
    {
        public async static Task<BitmapImage> LoadBitmapImage(SoundCloudTrack track, ImageQuality quality, DirectoryInfo albumDirectory)
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

        public static async Task<List<SoundCloudWebTrackResult>> Search(string searchText)
        {
            using (var web = new WebClient {Proxy = null})
            {
                var results = JsonConvert.DeserializeObject<List<ApiResult>>(await web.DownloadStringTaskAsync(string.Format("https://api.soundcloud.com/tracks?q={0}&client_id={1}", Utilities.GeneralHelper.EscapeTitleName(searchText), SensitiveInformation.SoundCloudKey)));
                return results.Where(x => x.streamable).Select(x => new SoundCloudWebTrackResult
                {
                    Duration = TimeSpan.FromMilliseconds(x.duration),
                    Year =  x.release_year != null ? uint.Parse(x.release_year.ToString()) : (uint)DateTime.Parse(x.created_at).Year,
                    Title = x.title,
                    Uploader = x.user.username,
                    Result = x,
                    Views = x.playback_count,
                    ImageUrl = x.artwork_url,
                    Url = x.permalink_url,
                    Genres = x.genre,
                }).ToList();
            }
        }
    }
}
