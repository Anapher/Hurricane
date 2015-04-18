using System;
using System.Windows.Media;
using Hurricane.Music.Download;
using Hurricane.Music.Track.WebApi.YouTubeApi.DataClasses;
using Hurricane.Utilities;

namespace Hurricane.Music.Track.WebApi.YouTubeApi
{
    public class YouTubeWebTrackResult : WebTrackResultBase
    {
        public override ProviderName ProviderName
        {
            get { return ProviderName.YouTube; }
        }

        public override PlayableBase ToPlayable()
        {
            var ytresult = (Entry)Result;
            var result = new YouTubeTrack
            {
                YouTubeId = YouTubeTrack.GetYouTubeIdFromLink(Url),
                TimeAdded = DateTime.Now,
                IsChecked = false
            };

            result.LoadInformation(ytresult);
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
            get { return Title.ToEscapedFilename(); }
        }

        public override DownloadMethod DownloadMethod
        {
            get { return DownloadMethod.youtube_dl; }
        }
    }
}
