using System;
using System.Threading.Tasks;
using System.Windows.Media;
using Hurricane.Music.Download;

namespace Hurricane.Music.Track.WebApi.SoundCloudApi
{
    class SoundCloudWebTrackResult : WebTrackResultBase
    {
        public override ProviderName ProviderName
        {
            get { return ProviderName.SoundCloud; }
        }

        public async override Task<PlayableBase> ToPlayable()
        {
            var result = (ApiResult) Result;
            var newtrack = new SoundCloudTrack
            {
                Url = Url,
                TimeAdded = DateTime.Now
            };

            if (_soundSourceInfo == null && !(await CheckIfAvailable()))
            {
                throw new Exception();
            }

            newtrack.LoadInformation(result, _soundSourceInfo);
            return newtrack;
        }

        public override GeometryGroup ProviderVector
        {
            get { return SoundCloudTrack.GetProviderVector(); }
        }

        public override bool CanDownload
        {
            get { return ((ApiResult)Result).downloadable && !string.IsNullOrEmpty(((ApiResult)Result).download_url); }
        }

        public override string DownloadParameter
        {
            get { return ((ApiResult)Result).id.ToString(); }
        }

        public override string DownloadFilename
        {
            get { return Utilities.GeneralHelper.EscapeFilename(Title) + ".mp3"; }
        }

        public override DownloadMethod DownloadMethod
        {
            get { return DownloadMethod.SoundCloud; }
        }

        private SoundSourceInfo _soundSourceInfo;
        public async override Task<bool> CheckIfAvailable()
        {
            var newtrack = new SoundCloudTrack { SoundCloudID = ((ApiResult)Result).id };
            
            try
            {
                using (var x = await newtrack.GetSoundSource())
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
