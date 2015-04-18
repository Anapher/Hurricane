using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CSCore;
using CSCore.Codecs;
using Newtonsoft.Json;
using Hurricane.Music.Download;
using Hurricane.Music.MusicCover;
using Hurricane.Music.Track.WebApi.SoundCloudApi;
using Hurricane.Settings;
using Hurricane.Utilities;

namespace Hurricane.Music.Track
{
    public class SoundCloudTrack : StreamableBase
    {
        public int SoundCloudID { get; set; }
        public string Url { get; set; }
        public string ArtworkUrl { get; set; }
        public bool IsDownloadable { get; set; }
        public bool Downloadable { get; set; }

        public async override Task<bool> LoadInformation()
        {
            using (var web = new WebClient { Proxy = null })
            {
                var result = JsonConvert.DeserializeObject<ApiResult>(await web.DownloadStringTaskAsync(string.Format("https://api.soundcloud.com/tracks/{0}.json?client_id={1}", SoundCloudID, SensitiveInformation.SoundCloudKey)));
                return LoadInformation(result);
            }
        }

        public bool LoadInformation(ApiResult result)
        {
            if (!result.IsStreamable) return false;
            Year = result.release_year != null
                ? uint.Parse(result.release_year.ToString())
                : (uint)DateTime.Parse(result.created_at).Year;
            Title = result.title;
            ArtworkUrl = result.artwork_url != null ? result.artwork_url.Replace("large.jpg", "{0}.jpg") : string.Empty;
            Artist = result.user.username;
            Genres = new List<Genre> { StringToGenre(result.genre) };
            SoundCloudID = result.id;
            Uploader = result.user.username;
            Downloadable = result.downloadable;
            SetDuration(TimeSpan.FromSeconds(result.duration));
            return true;
        }

        #region Image

        protected async override Task LoadImage(DirectoryInfo albumCoverDirectory)
        {
            if (albumCoverDirectory.Exists)
            {
                var regex =
                    new Regex(HurricaneSettings.Instance.Config.LoadAlbumCoverFromInternet
                        ? string.Format("^{0}_{1}.", SoundCloudID,
                            SoundCloudApi.GetQualityModifier(HurricaneSettings.Instance.Config.DownloadAlbumCoverQuality))
                        : string.Format("^{0}_", SoundCloudID));

                var imageFile = albumCoverDirectory.GetFiles().FirstOrDefault(item => regex.IsMatch(item.Name.ToLower()));
                if (imageFile != null)
                {
                    Image = new BitmapImage(new Uri(imageFile.FullName));
                    return;
                }

                Image = MusicCoverManager.GetAlbumImage(this, albumCoverDirectory);
                if (Image != null) return;
            }


            if (HurricaneSettings.Instance.Config.LoadAlbumCoverFromInternet)
            {
                try
                {
                    if (!string.IsNullOrEmpty(ArtworkUrl))
                    {
                        Image = await SoundCloudApi.LoadBitmapImage(this, HurricaneSettings.Instance.Config.DownloadAlbumCoverQuality, albumCoverDirectory);
                        if (Image != null) return;
                    }

                    Image = await MusicCoverManager.LoadCoverFromWeb(this, albumCoverDirectory, Uploader != Artist);
                }
                catch (WebException)
                {
                    //Happens, doesn't matter
                }
            }
        }

        #endregion

        public override Task<IWaveSource> GetSoundSource()
        {
            return
                Task.Run(() => CutWaveSource(CodecFactory.Instance.GetCodec(
                    new Uri(
                        string.Format("https://api.soundcloud.com/tracks/{0}/stream?client_id={1}", SoundCloudID,
                            SensitiveInformation.SoundCloudKey)))));
        }

        public override bool Equals(PlayableBase other)
        {
            if (other == null) return false;
            if (GetType() != other.GetType()) return false;
            return SoundCloudID == ((SoundCloudTrack)other).SoundCloudID;
        }

        public override void OpenTrackLocation()
        {
            Process.Start(Url);
        }

        private static GeometryGroup _geometryGroup;
        public static GeometryGroup GetProviderVector()
        {
            if (_geometryGroup == null)
            {
                _geometryGroup = new GeometryGroup();
                _geometryGroup.Children.Add((Geometry)Application.Current.Resources["VectorSoundCloud"]);
            }
            return _geometryGroup;
        }

        public override GeometryGroup ProviderVector
        {
            get { return GetProviderVector(); }
        }

        public override string DownloadParameter
        {
            get { return SoundCloudID.ToString(); }
        }

        public override string DownloadFilename
        {
            get { return Title.ToEscapedFilename(); }
        }

        public override DownloadMethod DownloadMethod
        {
            get { return DownloadMethod.SoundCloud; }
        }

        public override bool CanDownload
        {
            get { return Downloadable; }
        }

        public override string Link
        {
            get { return Url; }
        }

        public override string Website
        {
            get { return "https://soundcloud.com/"; }
        }

        public override bool IsInfinityStream
        {
            get { return false; }
        }
    }
}