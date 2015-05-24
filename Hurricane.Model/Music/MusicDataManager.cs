using System.Collections.ObjectModel;
using Hurricane.Model.DataApi;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;

namespace Hurricane.Model.Music
{
    public class MusicDataManager
    {
        public MusicDataManager()
        {
            Tracks = new ObservableCollection<PlayableBase>();
            Playlists = new ObservableCollection<UserPlaylist>();
            LastfmApi = new LastfmApi();
        }

        public ObservableCollection<PlayableBase> Tracks { get; set; }
        public ObservableCollection<UserPlaylist> Playlists { get; set; }
        public MusicManager MusicManager { get; set; }
        public LastfmApi LastfmApi { get; set; }
    }
}