using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hurricane.Settings;
using Hurricane.Utilities;
using Hurricane.Music.Track;
using Hurricane.Music.Track.WebApi.SoundCloudApi;

namespace Hurricane.Music.MusicDatabase
{
    public class MusicCoverManager
    {
        public static BitmapImage GetImage(PlayableBase track, DirectoryInfo di)
        {
            if (string.IsNullOrEmpty(track.Album)) return null;
            if (di.Exists)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var item in di.GetFiles("*.png"))
                {
                    if (GeneralHelper.EscapeFilename(track.Album).ToLower() == Path.GetFileNameWithoutExtension(item.FullName).ToLower())
                    {
                        return new BitmapImage(new Uri(item.FullName));
                    }
                }
            }

            return null;
        }

        public static BitmapImage GetSoundCloudImage(SoundCloudTrack track, DirectoryInfo di, ImageQuality quality, bool checkQuality)
        {
            string name = string.Format("{0}_{1}.jpg", track.SoundCloudID, SoundCloudApi.GetQualityModifier(quality));
            if (di.Exists)
            {
                return di.GetFiles("*.jpg").Where(item => !checkQuality || item.Name.ToLower() == name).Select(item => new BitmapImage(new Uri(item.FullName))).FirstOrDefault();
            }
            return null;
        }

        public static BitmapImage GetYouTubeImage(YouTubeTrack track, DirectoryInfo di)
        {
            if (di.Exists)
            {
                return di.GetFiles("*.jpg").Where(item => item.Name.ToLower() == track.YouTubeId.ToLower()).Select(item => new BitmapImage(new Uri(item.FullName))).FirstOrDefault();
            }
            return null;
        }

        public static async Task<BitmapImage> LoadCoverFromWeb(PlayableBase track, DirectoryInfo di, bool UseArtist = true)
        {
            var config = HurricaneSettings.Instance.Config;
            if (config.SaveCoverLocal)
            {
                if (!di.Exists) di.Create();
            }

            return await LastfmAPI.GetImage(config.DownloadAlbumCoverQuality, config.SaveCoverLocal, di, track, config.TrimTrackname, UseArtist);
        }
    }
}
