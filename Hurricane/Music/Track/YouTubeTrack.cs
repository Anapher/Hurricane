using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using CSCore;
using CSCore.Codecs;
using Hurricane.Settings;
using System.Windows;
using Hurricane.Music.MusicDatabase;
using Hurricane.Music.Track.YouTubeApi;
using Newtonsoft.Json;

namespace Hurricane.Music.Track
{
    public class YouTubeTrack : StreamableBase
    {
        public string YouTubeLink { get; set; }
        public string YouTubeId { get; set; }
        public string ThumbnailUrl { get; set; }

        public async override Task<bool> LoadInformation()
        {
            var youtubeMatch = Regex.Match(YouTubeLink, @"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)");
            if (!youtubeMatch.Success) return false;
            YouTubeId = youtubeMatch.Groups[youtubeMatch.Groups.Count - 1].Value;
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
            ThumbnailUrl = result.data.thumbnail.hqDefault;

            using (var soundSource = await GetSoundSource())
            {
                kHz = soundSource.WaveFormat.SampleRate / 1000;
                SetDuration(soundSource.GetLength());
            }
            return true;
        }

        public async override void Load()
        {
            IsLoadingImage = true;

            if (Image == null)
            {
                var diAlbumCover = new DirectoryInfo(HurricaneSettings.Instance.CoverDirectory);
                Image = MusicCoverManager.GetYouTubeImage(this, diAlbumCover);
                if (Image == null && HurricaneSettings.Instance.Config.LoadAlbumCoverFromInternet)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(ThumbnailUrl))
                        {
                            Image = await YouTubeApi.YouTubeApi.LoadBitmapImage(this, diAlbumCover);
                        }
                        else
                        {
                            Image = await MusicCoverManager.LoadCoverFromWeb(this, diAlbumCover).ConfigureAwait(false);
                        }
                    }
                    catch (WebException)
                    {
                        //Happens, doesn't matter
                    }
                }
            }

            IsLoadingImage = false;
            OnImageLoadComplete();
        }

        public override void OpenTrackLocation()
        {
            Process.Start(YouTubeLink);
        }

        private bool _tryagain;
        public async override Task<IWaveSource> GetSoundSource()
        {
            using (var p = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = Path.Combine(HurricaneSettings.Instance.BaseDirectory, "youtube-dl.exe"),
                    Arguments = string.Format("-g {0}", YouTubeLink)
                }
            })
            {
                p.Start();
                var url = await p.StandardOutput.ReadToEndAsync();
                if (string.IsNullOrEmpty(url))
                {
                    if (_tryagain) throw new Exception(url);
                    _tryagain = true;
                    return await GetSoundSource();
                }
                if (!url.ToLower().StartsWith("error"))
                {
                    return await Task.Run(() => CodecFactory.Instance.GetCodec(new Uri(url)));
                }
                throw new Exception(url);
            }
        }

        public override bool Equals(PlayableBase other)
        {
            if (other == null) return false;
            if (GetType() != other.GetType()) return false;
            return YouTubeLink == ((YouTubeTrack)other).YouTubeLink;
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
    }
}
