using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hurricane.Model.AudioEngine;
using Hurricane.Model.Music.Imagment;

namespace Hurricane.Model.Music.Playable
{
    public interface IPlayable
    {
        string Title { get; }
        string Artist { get; }
        ImageProvider Cover { get; }
        bool IsAvailable { get; }
        bool IsPlaying { get; set; }

        Task<IPlaySource> GetSoundSource();
        Task LoadImage();
    }
}