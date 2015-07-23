using System.Windows.Media;

namespace Hurricane.Model.Music.Playable
{
    public interface IStreamable : IPlayable
    {
        string Url { get; }
        string ProviderUrl { get; }
        string ProviderName { get; }
        Geometry ProviderIcon { get; }
    }
}
