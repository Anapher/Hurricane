using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hurricane.Model.Data;
using Hurricane.Model.DataApi;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Settings;

namespace Hurricane.Model.Music
{
    public class MusicDataManager : IDisposable
    {
        private const string ArtistFilename = "Artists.xml";
        private const string TracksFilename = "Tracks.xml";
        private const string PlaylistsFilename = "Playlists.xml";
        private const string AlbumsFilename = "Albums.xml";

        public MusicDataManager()
        {
            Playlists = new PlaylistProvider();
            LastfmApi = new LastfmApi();
            MusicManager = new MusicManager();
            Artists = new ArtistProvider();
            Tracks = new TrackProvider();
            Albums = new AlbumsProvider();
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
        public AlbumsProvider Albums { get; set; }

        public async Task Load(string rootFolder)
        {
            var artistFileInfo = new FileInfo(Path.Combine(rootFolder, ArtistFilename));
            var tracksFileInfo = new FileInfo(Path.Combine(rootFolder, TracksFilename));
            var playlistsFileInfo = new FileInfo(Path.Combine(rootFolder, PlaylistsFilename));
            var albumsFileInfo = new FileInfo(Path.Combine(rootFolder, AlbumsFilename));

            if (artistFileInfo.Exists)
                await Artists.LoadFromFile(artistFileInfo.FullName);

            if (albumsFileInfo.Exists)
                await Albums.LoadFromFile(albumsFileInfo.FullName);

            if (tracksFileInfo.Exists)
            {
                await Tracks.LoadFromFile(tracksFileInfo.FullName);
                Tracks.LoadData(Artists, Albums);
            }

            if (playlistsFileInfo.Exists)
                await Playlists.LoadFromFile(playlistsFileInfo.FullName, Tracks.Collection);

            LoadSettings();
        }

        public void Save(string rootFolder)
        {
            Playlists.SaveToFile(Path.Combine(rootFolder, PlaylistsFilename), Tracks.Collection);
            Artists.SaveToFile(Path.Combine(rootFolder, ArtistFilename));
            Tracks.SaveToFile(Path.Combine(rootFolder, TracksFilename));
            Albums.SaveToFile(Path.Combine(rootFolder, AlbumsFilename));

            CopyToSettings();
        }

        private void LoadSettings()
        {
            var settings = SettingsManager.Current;
            MusicManager.AudioEngine.Volume = settings.Volume;
            MusicManager.AudioEngine.SoundOutProvider.SetSoundOut(settings.SoundOutMode,
                settings.SoundOutDevice);

            if (settings.CurrentTrack != Guid.Empty && settings.CurrentPlaylist != Guid.Empty)
            {
                IPlaylist playlist;
                if (settings.CurrentPlaylist == TrackProvider.Id)
                {
                    playlist = Tracks;
                }
                else
                {
                    playlist = Playlists.Playlists.First(x => x.Id == settings.CurrentPlaylist);
                }

                MusicManager.OpenPlayable(Tracks.Collection[settings.CurrentTrack], playlist, false);
            }
        }

        private void CopyToSettings()
        {
            var settings = SettingsManager.Current;

            settings.Volume = MusicManager.AudioEngine.Volume;
            if (MusicManager.AudioEngine.SoundOutProvider.CurrentSoundOutDevice != null)
            {
                settings.SoundOutDevice =
                    MusicManager.AudioEngine.SoundOutProvider.CurrentSoundOutDevice.Id;
                settings.SoundOutMode =
                    MusicManager.AudioEngine.SoundOutProvider.CurrentSoundOutDevice.SoundOutMode;
            }

            if (MusicManager.CurrentTrack != null)
            {
                //settings.CurrentTrack = Tracks.Collection.First(x => x.Value == MusicManager.CurrentTrack).Key;
                //settings.CurrentPlaylist = MusicManager.CurrentPlaylist.id;
            }
        }
    }
}