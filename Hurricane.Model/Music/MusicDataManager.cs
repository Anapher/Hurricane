using System;
using System.IO;
using System.Threading.Tasks;
using Hurricane.Model.Data;
using Hurricane.Model.DataApi;

namespace Hurricane.Model.Music
{
    public class MusicDataManager : IDisposable
    {
        private const string ArtistFilename = "Artists.xml";
        private const string TracksFilename = "Tracks.xml";
        private const string PlaylistsFilename = "Playlists.xml";

        public MusicDataManager()
        {
            Playlists = new PlaylistProvider();
            LastfmApi = new LastfmApi();
            MusicManager = new MusicManager();
            Artists = new ArtistProvider();
            Tracks = new TrackProvider();
        }

        public void Dispose()
        {
            MusicManager.Dispose();
        }

        public TrackProvider Tracks { get; set; }
        public PlaylistProvider Playlists { get; }
        public MusicManager MusicManager { get; }
        public LastfmApi LastfmApi { get; }
        public ArtistProvider Artists { get; }

        public async Task Load(string rootFolder)
        {
            var artistFileInfo = new FileInfo(Path.Combine(rootFolder, ArtistFilename));
            var tracksFileInfo = new FileInfo(Path.Combine(rootFolder, TracksFilename));
            var playlistsFileInfo = new FileInfo(Path.Combine(rootFolder, PlaylistsFilename));

            if (artistFileInfo.Exists)
                await Artists.LoadFromFile(artistFileInfo.FullName);

            if (tracksFileInfo.Exists)
            {
                await Tracks.LoadFromFile(tracksFileInfo.FullName);
                Tracks.LoadData(Artists);
            }

            if (playlistsFileInfo.Exists)
                await Playlists.LoadFromFile(playlistsFileInfo.FullName, Tracks.Collection);
        }

        public void Save(string rootFolder)
        {
            Playlists.SaveToFile(Path.Combine(rootFolder, PlaylistsFilename), Tracks.Collection);
            Artists.SaveToFile(Path.Combine(rootFolder, ArtistFilename));
            Tracks.SaveToFile(Path.Combine(rootFolder, TracksFilename));
        }
    }
}