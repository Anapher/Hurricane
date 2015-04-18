using System;
using System.Threading.Tasks;
using System.Windows.Media;
using Hurricane.Music.Download;

namespace Hurricane.Music.Track.WebApi.VkontakteApi
{
    class VkontakteWebTrackResult : WebTrackResultBase
    {
        public Audio SearchResult { get; set; }

        public override ProviderName ProviderName
        {
            get { return ProviderName.Vkontakte; }
        }

        public override PlayableBase ToPlayable()
        {
            var track = new VkontakteTrack()
            {
                IsChecked = false,
                TimeAdded = DateTime.Now
            };

            track.LoadInformation(SearchResult);
            return track;
        }

        public override GeometryGroup ProviderVector
        {
            get { return VkontakteTrack.GetProviderVector(); }
        }

        public override string DownloadParameter
        {
            get { throw new NotImplementedException(); }
        }

        public override string DownloadFilename
        {
            get { throw new NotImplementedException(); }
        }

        public override DownloadMethod DownloadMethod
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanDownload
        {
            get { return false; }
        }

        public async override Task DownloadImage()
        {
            
        }
    }
}