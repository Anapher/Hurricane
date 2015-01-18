using System.Threading.Tasks;
using System.Windows.Media;
using Hurricane.Music.Download;

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
                YouTubeId = YouTubeTrack.GetYouTubeIdFromLink(Url)
            };
            await result.LoadInformation();
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
            get {; return DownloadMethod.youtube_dl; }
        }
    }
}
