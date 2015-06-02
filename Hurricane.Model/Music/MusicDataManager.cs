using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Hurricane.Model.Data;
using Hurricane.Model.DataApi;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using Hurricane.Utilities;

namespace Hurricane.Model.Music
{
    public class MusicDataManager : IDisposable
    {
        private const string ArtistFilename = "Artists.xml";

        public MusicDataManager()
        {
            Tracks = new ObservableDictionary<Guid, PlayableBase>();
            Playlists = new ObservableCollection<UserPlaylist>();
            LastfmApi = new LastfmApi();
            MusicManager = new MusicManager();
            ArtistManager = new ArtistManager();
        }

        public void Dispose()
        {
            MusicManager.Dispose();
        }

        public ObservableDictionary<Guid, PlayableBase> Tracks { get; }
        public ObservableCollection<UserPlaylist> Playlists { get; }
        public MusicManager MusicManager { get; }
        public LastfmApi LastfmApi { get; }
        public ArtistManager ArtistManager { get; }

        public async Task Load(string rootFolder)
        {
            var artistFileInfo = new FileInfo(Path.Combine(rootFolder, ArtistFilename));
            if (artistFileInfo.Exists)
                await ArtistManager.LoadFromFile(artistFileInfo.FullName);
        }

        public async Task Save(string rootFolder)
        {
            await ArtistManager.SaveToFile(Path.Combine(rootFolder, ArtistFilename));
        }
    }
}