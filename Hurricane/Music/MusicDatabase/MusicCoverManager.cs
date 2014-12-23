using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
                    if (Utilities.GeneralHelper.EscapeFilename(track.Album).ToLower() == System.IO.Path.GetFileNameWithoutExtension(item.FullName).ToLower())
                    {
                        return new BitmapImage(new Uri(item.FullName));
                    }
                }
            }

            return null;
        }

        public static async Task<BitmapImage> LoadCoverFromWeb(Track track, DirectoryInfo di)
        {
            var config = Settings.HurricaneSettings.Instance.Config;
            if (config.SaveCoverLocal)
            {
                if (!di.Exists) di.Create();
            }

            return await MusicDatabase.LastfmAPI.GetImage(track.Title, track.Artist, config.DownloadAlbumCoverQuality, config.SaveCoverLocal, di, track, config.TrimTrackname);
        }
    }
}
