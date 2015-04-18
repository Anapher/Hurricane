using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CSCore;
using CSCore.Codecs;
using Newtonsoft.Json;
using Hurricane.Music.Download;
using Hurricane.Music.MusicCover;
using Hurricane.Music.Track.WebApi.GroovesharkApi;
using Hurricane.Settings;

namespace Hurricane.Music.Track
{
    public class GroovesharkTrack : StreamableBase
    {
        public string TinySongUrl { get; set; }
        public int SongId { get; set; }

        public override GeometryGroup ProviderVector
        {
            get { return GetProviderVector(); }
        }

        private static GeometryGroup _geometryGroup;
        public static GeometryGroup GetProviderVector()
        {
            if (_geometryGroup == null)
            {
                _geometryGroup = new GeometryGroup();
                _geometryGroup.Children.Add((Geometry)Application.Current.Resources["VectorGrooveshark"]);
            }
            return _geometryGroup;
        }

        public override string Link
        {
            get { return TinySongUrl; }
        }

        public override string Website
        {
            get { return "http://grooveshark.com/"; }
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
            using (var wc = new WebClient {Proxy = null})
            {
                return
                    LoadInformation(
                        JsonConvert.DeserializeObject<SearchResult>(
                            await
                                wc.DownloadStringTaskAsync(
                                    string.Format("http://tinysong.com/b/{0}+{1}?format=json&key={2}",
                                        Title.Replace(" ", "+"), Artist.Replace(" ", "+"),
                                        SensitiveInformation.TinySongKey))));
            }
        }

        public bool LoadInformation(SearchResult searchResult)
        {
            Title = searchResult.SongName;
            SongId = searchResult.SongID;
            Artist = searchResult.ArtistName;
            Album = searchResult.AlbumName;
            return true;
        }

        public override void OpenTrackLocation()
        {
            Process.Start(Link);
        }

        public async override Task<IWaveSource> GetSoundSource()
        {
            //var stream = await youtube_dl.Instance.GetGroovesharkStream(TinySongUrl);
            //return new DmoMp3Decoder(stream);
            return CodecFactory.Instance.GetCodec(@"D:\Musik\Französische Musik\Nolwenn Leroy - La jument de Michao - clip officiel.mp3");
        }

        public override bool Equals(PlayableBase other)
        {
            if (other == null) return false;
            if (GetType() != other.GetType()) return false;
            return TinySongUrl == ((GroovesharkTrack)other).TinySongUrl;
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

        public override bool IsInfinityStream
        {
            get { return false; }
        }
    }
}