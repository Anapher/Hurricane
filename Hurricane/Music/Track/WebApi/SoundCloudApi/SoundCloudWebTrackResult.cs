using System;
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

        public override PlayableBase ToPlayable()
        {
            var result = (ApiResult) Result;
            var newtrack = new SoundCloudTrack
            {
                Url = Url,
                TimeAdded = DateTime.Now,
                IsChecked = false
            };

            newtrack.LoadInformation(result);
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
            get { return Utilities.GeneralHelper.EscapeFilename(Title); }
        }

        public override DownloadMethod DownloadMethod
        {
            get { return DownloadMethod.SoundCloud; }
        }
    }
}
