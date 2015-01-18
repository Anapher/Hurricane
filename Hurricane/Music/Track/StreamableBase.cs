using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using CSCore;
using Hurricane.ViewModelBase;

namespace Hurricane.Music.Track
{
    public abstract class StreamableBase : PlayableBase, IDownloadable
    {
        public override abstract Task<bool> LoadInformation();
        public override abstract void Load();
        public override abstract void OpenTrackLocation();
        public override abstract Task<IWaveSource> GetSoundSource();
        public override abstract bool Equals(PlayableBase other);

        public override TrackType TrackType
        {
            get { return TrackType.Stream; }
        }

        public override bool TrackExists
        {
            get
            {
                return true;
            }
        }

        public abstract GeometryGroup ProviderVector { get; }
        public string Uploader { get; set; }
        public abstract string Link { get; }
        public abstract string Website { get; }

        //IDownloadable
        public abstract string DownloadParameter { get; }
        public abstract string DownloadFilename { get; }
        public abstract Download.DownloadMethod DownloadMethod { get; }
        public abstract bool CanDownload { get; }

        private RelayCommand _openLinkCommand;
        public RelayCommand OpenLinkCommand
        {
            get { return _openLinkCommand ?? (_openLinkCommand = new RelayCommand(parameter => { Process.Start(Link); })); }
        }

        private RelayCommand _openWebsiteCommand;
        public RelayCommand OpenWebsiteCommand
        {
            get { return _openWebsiteCommand ?? (_openWebsiteCommand = new RelayCommand(parameter => { Process.Start(Website); })); }
        }
    }
}
