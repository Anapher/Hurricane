using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hurricane.Model.DataApi;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.Utilities;

namespace Hurricane.Model.Music
{
    public class MusicDataManager : IDisposable
    {
        public MusicDataManager()
        {
            Tracks = new ObservableDictionary<Guid, PlayableBase>();
            Playlists = new ObservableCollection<UserPlaylist>();
            LastfmApi = new LastfmApi();
            MusicManager = new MusicManager();
            Artists = new Dictionary<Guid, Artist>();
        }

        public void Dispose()
        {
            MusicManager.Dispose();
        }

        public ObservableDictionary<Guid, PlayableBase> Tracks { get; }
        public ObservableCollection<UserPlaylist> Playlists { get; }
        public MusicManager MusicManager { get; }
        public LastfmApi LastfmApi { get; }
        public Dictionary<Guid, Artist> Artists { get; }
    }
}