using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Hurricane.Music.Data
{
    public interface IPlaylist
    {
        ICollectionView ViewSource { get; set; }

        ObservableCollection<Track> Tracks { get; }

        string SearchText { get; set; }
        string Name { get; set; }
        bool CanEdit { get; }
        bool ContainsMissingTracks { get; }

        Track GetRandomTrack(Track currentTrack);

        void AddTrack(Track track);
        void RemoveTrack(Track track);
        void Clear();
        void RemoveMissingTracks();
    }
}
