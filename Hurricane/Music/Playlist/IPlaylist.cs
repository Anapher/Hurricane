using System.Collections.ObjectModel;
using System.ComponentModel;
using Hurricane.Music.Track;

namespace Hurricane.Music.Playlist
{
    public interface IPlaylist
    {
        ICollectionView ViewSource { get; set; }

        ObservableCollection<PlayableBase> Tracks { get; }

        string SearchText { get; set; }
        string Name { get; set; }
        bool CanEdit { get; }
        bool ContainsMissingTracks { get; }
        bool ContainsDownloadableStreams { get; }

        PlayableBase GetRandomTrack(PlayableBase currentTrack);

        void AddTrack(PlayableBase track);
        void RemoveTrack(PlayableBase track);
        void Clear();
        void RemoveMissingTracks();
    }
}
