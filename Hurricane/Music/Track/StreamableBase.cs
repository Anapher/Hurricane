using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CSCore;

namespace Hurricane.Music.Track
{
    public abstract class StreamableBase : PlayableBase
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
    }
}
