using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CSCore;
using CSCore.Codecs;
using Hurricane.Music.Download;
using Hurricane.Music.MusicCover;
using Hurricane.Music.Track.WebApi.VkontakteApi;
using Hurricane.Settings;

namespace Hurricane.Music.Track
{
    public class VkontakteTrack : StreamableBase
    {
        public string StreamUrl { get; set; }

        private static GeometryGroup _geometryGroup;
        public static GeometryGroup GetProviderVector()
        {
            if (_geometryGroup == null)
            {
                _geometryGroup = new GeometryGroup();
                _geometryGroup.Children.Add((Geometry)Application.Current.Resources["VectorVkontakte"]);
            }
            return _geometryGroup;
        }

        public override string Link
        {
            get { return StreamUrl; }
        }

        public override string Website
        {
            get { return "https://vk.com/"; }
        }

        public override bool IsInfinityStream
        {
            get { return false; }
        }

        public override string DownloadParameter
        {
            get { throw new NotImplementedException(); }
        }

        public override string DownloadFilename
        {
            get { throw new NotImplementedException(); }
        }

        public override DownloadMethod DownloadMethod
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanDownload
        {
            get { return false; }
        }

        public async override Task<bool> LoadInformation()
        {
            return true;
        }

        public bool LoadInformation(Audio result)
        {
            SetDuration(TimeSpan.FromSeconds(result.duration));
            Title = result.title;
            Artist = result.artist;
            Album = result.album;
            StreamUrl = result.url;
            Genres = new List<Genre> { (Genre)Enum.Parse(typeof(Genre), result.genre.ToString()) };
            return true;
        }

        public override void OpenTrackLocation()
        {
            Process.Start(StreamUrl);
        }

        public override Task<IWaveSource> GetSoundSource()
        {
            return
                Task.Run(() => CutWaveSource(CodecFactory.Instance.GetCodec(
                    new Uri(
                        StreamUrl))));
        }

        public override bool Equals(PlayableBase other)
        {
            if (other == null) return false;
            if (GetType() != other.GetType()) return false;
            return StreamUrl == ((VkontakteTrack)other).StreamUrl;
        }

        protected async override Task LoadImage(DirectoryInfo albumCoverDirectory)
        {
            if (albumCoverDirectory.Exists)
            {
                Image = MusicCoverManager.GetAlbumImage(this, albumCoverDirectory);
                if (Image != null) return;
            }

            if (HurricaneSettings.Instance.Config.LoadAlbumCoverFromInternet)
            {
                try
                {
                    Image = await MusicCoverManager.LoadCoverFromWeb(this, albumCoverDirectory, Uploader != Artist);
                }
                catch (WebException)
                {
                    //Happens, doesn't matter
                }
            }
        }

        public override GeometryGroup ProviderVector
        {
            get { return GetProviderVector(); }
        }
    }
}