using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hurricane.Music.MusicCover.APIs.Lastfm;
using Hurricane.Music.Track;
using Hurricane.Music.Track.WebApi.SoundCloudApi;
using Hurricane.Settings;
using Hurricane.Utilities;

namespace Hurricane.Music.MusicCover
{
    public class MusicCoverManager
    {
        public static BitmapImage GetAlbumImage(PlayableBase track, DirectoryInfo di)
        {
            if (string.IsNullOrEmpty(track.Album)) return null;
            if (di.Exists)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var item in di.GetFiles("*.png"))
                {
                    if (track.Album.ToEscapedFilename().ToLower() == Path.GetFileNameWithoutExtension(item.FullName).ToLower())
                    {
                        return new BitmapImage(new Uri(item.FullName));
                    }
                }
            }

            return null;
        }

        public static BitmapImage GetTrackImage(PlayableBase track, DirectoryInfo di)
        {
            if (di.Exists)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var item in di.GetFiles("*.png"))
                {
                    if (track.AuthenticationCode.ToString() == Path.GetFileNameWithoutExtension(item.FullName).ToLower())
                    {
                        return new BitmapImage(new Uri(item.FullName));
                    }
                }
            }

            return null;
        }

        public static async Task<BitmapImage> LoadCoverFromWeb(PlayableBase track, DirectoryInfo di, bool useArtist = true)
        {
            var config = HurricaneSettings.Instance.Config;
            if (config.SaveCoverLocal)
            {
                if (!di.Exists) di.Create();
            }

            return await LastfmApi.GetImage(config.DownloadAlbumCoverQuality, config.SaveCoverLocal, di, track, config.TrimTrackname, useArtist);
        }
    }
}
