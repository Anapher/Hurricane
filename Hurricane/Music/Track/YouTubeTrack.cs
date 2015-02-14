using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using CSCore;
using CSCore.Codecs;
using Hurricane.Settings;
using System.Windows;
using Hurricane.Music.Download;
using Hurricane.Music.MusicDatabase;
using Hurricane.Music.Track.WebApi.YouTubeApi;
using Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses;
using Newtonsoft.Json;

namespace Hurricane.Music.Track
{
    public class YouTubeTrack : StreamableBase
    {
        public string YouTubeId { get; set; }

        public string ThumbnailUrl
        {
            get { return string.Format("https://img.youtube.com/vi/{0}/mqdefault.jpg", YouTubeId); }
        }

        public static string GetYouTubeIdFromLink(string youTubeLink)
        {
            var youtubeMatch = Regex.Match(youTubeLink, @"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)(?<id>[a-zA-Z0-9-_]+)");
            if (!youtubeMatch.Success) return string.Empty;
            return youtubeMatch.Groups["id"].Value;
        }

        public async override Task<bool> LoadInformation()
        {
            using (var client = new WebClient {Proxy = null})
            {
                return await LoadInformation(JsonConvert.DeserializeObject<SingleVideoSearchResult>(await client.DownloadStringTaskAsync(string.Format("http://gdata.youtube.com/feeds/api/videos/{0}?v=2&alt=jsonc", YouTubeId))));
            }
        }

        public async Task<bool> LoadInformation(SingleVideoSearchResult result)
        {
            Year = (uint) DateTime.Parse(result.data.uploaded).Year;
            Title = result.data.title;
            Artist = result.data.uploader;
            Uploader = result.data.uploader; //Because the user can change the artist

            using (var soundSource = await GetSoundSource())
            {
                kHz = soundSource.WaveFormat.SampleRate / 1000;
                SetDuration(soundSource.GetLength());
            }
            return true;
        }

        public bool LoadInformation(Entry ytResult, SoundSourceInfo soundSourceInfo)
        {
            Year = (uint)DateTime.Parse(ytResult.published.Date).Year;
            Title = ytResult.title.Name;
            Artist = ytResult.author.First().name.Text;
            Uploader = ytResult.author.First().name.Text;
            kHz = soundSourceInfo.kHz;
            SetDuration(soundSourceInfo.Duration);
            return true;
        }

        public async Task<bool> LoadInformation(Entry ytResult)
        {
            using (var soundSource = await GetSoundSource())
            {
              return LoadInformation(ytResult, SoundSourceInfo.FromSoundSource(soundSource));
            }
        }

        public override void OpenTrackLocation()
        {
            Process.Start(Link);
        }

        public async override Task<IWaveSource> GetSoundSource()
        {
            var streamUri = await youtube_dl.Instance.GetStreamUri(Link);
            return await Task.Run(() => CodecFactory.Instance.GetCodec(streamUri));
        }

        public override bool Equals(PlayableBase other)
        {
            if (other == null) return false;
            if (GetType() != other.GetType()) return false;
            return Link == ((YouTubeTrack)other).Link;
        }

        private static GeometryGroup _geometryGroup;
        public static GeometryGroup GetProviderVector()
        {
            if (_geometryGroup == null)
            {
                _geometryGroup = new GeometryGroup();
                _geometryGroup.Children.Add((Geometry)Application.Current.Resources["VectorYouTube"]);
            }
            return _geometryGroup;
        }

        public override GeometryGroup ProviderVector
        {
            get { return GetProviderVector(); }
        }

        public override string DownloadParameter
        {
            get { return Link; }
        }

        public override string DownloadFilename
        {
            get { return Utilities.GeneralHelper.EscapeFilename(Title) + ".m4a"; }
        }

        public override DownloadMethod DownloadMethod
        {
            get { return DownloadMethod.youtube_dl; }
        }

        public override bool CanDownload
        {
            get { return true; }
        }

        public override string Link
        {
            get { return string.Format("https://www.youtube.com/watch?v={0}", YouTubeId); }
        }

        public override string Website
        {
            get { return "https://www.youtube.com/"; }
        }

        protected async override Task LoadImage()
        {
            var diAlbumCover = new DirectoryInfo(HurricaneSettings.Instance.CoverDirectory);
            Image = MusicCoverManager.GetYouTubeImage(this, diAlbumCover);

            if (Image == null)
                Image = MusicCoverManager.GetImage(this, diAlbumCover);

            if (Image == null && HurricaneSettings.Instance.Config.LoadAlbumCoverFromInternet)
            {
                try
                {
                    if (!string.IsNullOrEmpty(ThumbnailUrl))
                    {
                        Image = await YouTubeApi.LoadBitmapImage(this, diAlbumCover);
                    }

                    if (Image == null)
                    {
                        Image = await MusicCoverManager.LoadCoverFromWeb(this, diAlbumCover, Uploader != Artist).ConfigureAwait(false);
                    }
                }
                catch (WebException)
                {
                    //Happens, doesn't matter
                }
            }
        }
    }
}
