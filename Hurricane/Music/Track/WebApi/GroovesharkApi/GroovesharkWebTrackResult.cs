using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hurricane.Music.Download;

namespace Hurricane.Music.Track.WebApi.GroovesharkApi
{
    class GroovesharkWebTrackResult : WebTrackResultBase
    {
        public SearchResult SearchResult { get; set; }

        public override ProviderName ProviderName
        {
            get { return ProviderName.Grooveshark; }
        }

        public override PlayableBase ToPlayable()
        {
            var track = new GroovesharkTrack
            {
                TinySongUrl = Url,
                IsChecked = false,
                TimeAdded = DateTime.Now
            };

            track.LoadInformation(SearchResult);
            return track;
        }

        public override GeometryGroup ProviderVector
        {
            get { return GroovesharkTrack.GetProviderVector(); }
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
            Image = new BitmapImage(new Uri("/Resources/App/bottom.png", UriKind.Relative));
        }
    }
}