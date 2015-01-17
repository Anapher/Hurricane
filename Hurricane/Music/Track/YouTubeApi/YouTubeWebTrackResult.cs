using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Hurricane.Music.Track.YouTubeApi
{
    public class YouTubeWebTrackResult : WebTrackResultBase
    {
        public override ProviderName ProviderName
        {
            get { return ProviderName.YouTube; }
        }

        public async override Task<PlayableBase> ToPlayable()
        {
            var result = new YouTubeTrack
            {
                YouTubeLink = Url
            };
            await result.LoadInformation();
            return result;
        }

        public override GeometryGroup ProviderVector
        {
            get { return YouTubeTrack.GetProviderVector(); }
        }

        public override string GetDownloadUrl()
        {
            return "";
        }

        public override bool CanDownload
        {
            get { return true; }
        }

        public override string GetFilename
        {
            get { return Utilities.GeneralHelper.EscapeFilename(Title) + ".mp3"; }
        }
    }
}
