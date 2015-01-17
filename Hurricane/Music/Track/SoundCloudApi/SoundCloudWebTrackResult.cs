using System.Threading.Tasks;
using System.Windows.Media;
using Hurricane.Settings;

namespace Hurricane.Music.Track.SoundCloudApi
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
            var newtrack = new SoundCloudTrack { Url = Url };
            await newtrack.LoadInformation(result);
            return newtrack;
        }

        public override GeometryGroup ProviderVector
        {
            get { return SoundCloudTrack.GetProviderVector(); }
        }

        public override string GetDownloadUrl()
        {
            return ((ApiResult)Result).download_url + "?client_id=" + SensitiveInformation.SoundCloudKey;
        }

        public override bool CanDownload
        {
            get { return ((ApiResult)Result).downloadable && !string.IsNullOrEmpty(((ApiResult)Result).download_url); }
        }

        public override string GetFilename
        {
            get { return Utilities.GeneralHelper.EscapeFilename(Title) + ".mp3"; }
        }
    }
}
