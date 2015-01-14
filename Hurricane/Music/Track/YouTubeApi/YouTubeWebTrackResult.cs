using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Hurricane.Music.Track.YouTubeApi
{
    public class YouTubeWebTrackResult : WebTrackResultBase
    {
        public async override Task<PlayableBase> ToPlayable()
        {
            var result = new YouTubeTrack
            {
                YouTubeLink = ((Entry)Result).link.First().href
            };
            await result.LoadInformation();
            return result;
        }

        public override GeometryGroup ProviderVector
        {
            get { return YouTubeTrack.GetProviderVector(); }
        }
    }
}
