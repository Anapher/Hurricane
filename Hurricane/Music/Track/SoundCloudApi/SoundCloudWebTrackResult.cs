using System.Threading.Tasks;
using System.Windows.Media;

namespace Hurricane.Music.Track.SoundCloudApi
{
    class SoundCloudWebTrackResult : WebTrackResultBase
    {
        public async override Task<PlayableBase> ToPlayable()
        {
            var result = (ApiResult) Result;
            var newtrack = new SoundCloudTrack { Url = result.permalink_url };
            await newtrack.LoadInformation(result);
            return newtrack;
        }

        public override GeometryGroup ProviderVector
        {
            get { return SoundCloudTrack.GetProviderVector(); }
        }
    }
}
