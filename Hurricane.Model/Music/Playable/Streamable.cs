using System.Threading.Tasks;
using System.Windows.Media;
using Hurricane.Model.AudioEngine;

namespace Hurricane.Model.Music.Playable
{
    public abstract class Streamable : PlayableBase, IStreamable
    {
        public override bool IsAvailable { get; } = true;
        public string Uploader { get; set; }
        public abstract override Task<IPlaySource> GetSoundSource();

        public abstract string Url { get;}
        public abstract string ProviderUrl { get; }
        public abstract string ProviderName { get; }
        public abstract Geometry ProviderIcon { get;}
    }
}