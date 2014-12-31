using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hurricane.Settings;
using Hurricane.Utilities;

namespace Hurricane.Music.MusicDatabase
{
    public class MusicCoverManager
    {
        public static BitmapImage GetImage(Track track, DirectoryInfo di)
        {
            if (di.Exists)
            {
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

        public static async Task<BitmapImage> LoadCoverFromWeb(Track track, DirectoryInfo di)
        {
            var config = HurricaneSettings.Instance.Config;
            if (config.SaveCoverLocal)
            {
                if (!di.Exists) di.Create();
            }

            return await LastfmAPI.GetImage(track.Title, track.Artist, config.DownloadAlbumCoverQuality, config.SaveCoverLocal, di, track, config.TrimTrackname);
        }
    }
}
