using System;
using System.Threading.Tasks;
using System.Windows.Media;
using Hurricane.Music.Download;
using Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses;

namespace Hurricane.Music.Track.WebApi.YouTubeApi
{
    public class YouTubeWebTrackResult : WebTrackResultBase
    {
        public override ProviderName ProviderName
        {
            get { return ProviderName.YouTube; }
        }

        public async override Task<PlayableBase> ToPlayable()
        {
            var ytresult = (Entry)Result;
            var result = new YouTubeTrack
            {
                YouTubeId = YouTubeTrack.GetYouTubeIdFromLink(Url),
                TimeAdded = DateTime.Now
            };

            if (_soundSourceInfo == null && !(await CheckIfAvailable()))
            {
                throw new Exception();
            }

            result.LoadInformation(ytresult, _soundSourceInfo);
            return result;
        }

        public override GeometryGroup ProviderVector
        {
            get { return YouTubeTrack.GetProviderVector(); }
        }

        public override bool CanDownload
        {
            get { return true; }
        }

        public override string DownloadParameter
        {
            get { return Url; }
        }

        public override string DownloadFilename
        {
            get { return Utilities.GeneralHelper.EscapeFilename(Title) + ".m4a"; }
        }

        public override DownloadMethod DownloadMethod
        {
            get { return DownloadMethod.youtube_dl; }
        }

        private SoundSourceInfo _soundSourceInfo;
        public override async Task<bool> CheckIfAvailable()
        {
            var result = new YouTubeTrack { YouTubeId = YouTubeTrack.GetYouTubeIdFromLink(Url) };
            try
            {
                using (var x = await result.GetSoundSource())
                {
                    _soundSourceInfo = SoundSourceInfo.FromSoundSource(x);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
