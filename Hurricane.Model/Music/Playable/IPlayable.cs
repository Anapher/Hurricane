using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hurricane.Model.AudioEngine;

namespace Hurricane.Model.Music.Playable
{
    public interface IPlayable
    {
        string Title { get; }
        string Artist { get; }
        BitmapImage Cover { get; }
        bool IsAvailable { get; }

        Task<IPlaySource> GetSoundSource();
        Task LoadImage();
    }
}