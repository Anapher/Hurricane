using System;
using System.Collections.ObjectModel;
using Hurricane.Model.DataApi;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;

namespace Hurricane.Model.Music
{
    public class MusicDataManager : IDisposable
    {
        public MusicDataManager()
        {
            Tracks = new ObservableCollection<PlayableBase>();
            Playlists = new ObservableCollection<UserPlaylist>();
            LastfmApi = new LastfmApi();
            MusicManager = new MusicManager();
        }

        public void Dispose()
        {
            MusicManager.Dispose();
        }

        public ObservableCollection<PlayableBase> Tracks { get; set; }
        public ObservableCollection<UserPlaylist> Playlists { get; set; }
        public MusicManager MusicManager { get; set; }
        public LastfmApi LastfmApi { get; set; }
    }
}