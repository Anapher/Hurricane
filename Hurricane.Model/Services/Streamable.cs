using System.Threading.Tasks;
using System.Windows.Media;
using Hurricane.Model.AudioEngine;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Services
{
    public abstract class Streamable : PlayableBase
    {
        public override bool IsAvailable { get; } = true;
        public string Uploader { get; set; }
        public abstract override Task<IPlaySource> GetSoundSource();

        public abstract string Url { get;}
        public abstract string ProviderUrl { get; }
        public abstract Geometry ProviderIcon { get;}
    }
}