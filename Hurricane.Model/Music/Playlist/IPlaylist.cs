using System.Threading.Tasks;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Music.Playlist
{
    public interface IPlaylist
    {
        Task<IPlayable> GetNextTrack(IPlayable currentTrack);
        Task<IPlayable> GetShuffleTrack();
        Task<IPlayable> GetPreviousTrack(IPlayable currentTrack);
        Task<IPlayable> GetLastTrack();
        bool ContainsPlayableTracks();
    }
}