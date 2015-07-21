using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Hurricane.Model.Data;
using Hurricane.Model.DataApi;
using Hurricane.Model.Music.Args;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.Model.Plugins.MusicStreaming;
using Hurricane.Model.Settings;
using Hurricane.Utilities;

namespace Hurricane.Model.Music
{
    public class MusicDataManager : IDisposable
    {
        private const string ArtistFilename = "Artists.xml";
        private const string TracksFilename = "Tracks.xml";
        private const string PlaylistsFilename = "Playlists.xml";
        private const string AlbumsFilename = "Albums.xml";
        private const string UserDataFilename = "UserData.xml";

        public MusicDataManager()
        {
            Playlists = new PlaylistProvider();
            LastfmApi = new LastfmApi();
            Artists = new ArtistProvider();
            Tracks = new TrackProvider();
            Albums = new AlbumsProvider();
            UserData = new UserDataProvider();
            MusicManager = new MusicManager();
            MusicManager.TrackChanged += MusicManager_TrackChanged;
            MusicStreamingPluginManager = new MusicStreamingPluginManager();
        }

        public void Dispose()
        {
            MusicManager.Dispose();
        }

        public TrackProvider Tracks { get; }
        public PlaylistProvider Playlists { get; }
        public MusicManager MusicManager { get; }
        public LastfmApi LastfmApi { get; }
        public ArtistProvider Artists { get; }
        public AlbumsProvider Albums { get; }
        public UserDataProvider UserData { get; }
        public MusicStreamingPluginManager MusicStreamingPluginManager { get; }

        public async Task Load(string rootFolder)
        {
            var artistFileInfo = new FileInfo(Path.Combine(rootFolder, ArtistFilename));
            var tracksFileInfo = new FileInfo(Path.Combine(rootFolder, TracksFilename));
            var playlistsFileInfo = new FileInfo(Path.Combine(rootFolder, PlaylistsFilename));
            var albumsFileInfo = new FileInfo(Path.Combine(rootFolder, AlbumsFilename));
            var userDataFileInfo = new FileInfo(Path.Combine(rootFolder, UserDataFilename));

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

            if (userDataFileInfo.Exists)
                await UserData.LoadFromFile(userDataFileInfo.FullName);

            LoadSettings();
        }

        public void Save(string rootFolder)
        {
            if (MusicManager.CurrentTrack != null)
                UserData.UserData.History.AddEntry(MusicManager.CurrentTrack,
                    MusicManager.AudioEngine.TimePlaySourcePlayed);

            Playlists.SaveToFile(Path.Combine(rootFolder, PlaylistsFilename), Tracks.Collection);
            Artists.SaveToFile(Path.Combine(rootFolder, ArtistFilename));
            Tracks.SaveToFile(Path.Combine(rootFolder, TracksFilename));
            Albums.SaveToFile(Path.Combine(rootFolder, AlbumsFilename));
            UserData.SaveToFile(Path.Combine(rootFolder, UserDataFilename));

            CopyToSettings();
        }

        public async Task<IPlayable> SearchTrack(string artist, string title)
        {
            PlayableBase result = null;
            var sw = Stopwatch.StartNew();
            foreach (var track in Tracks.Tracks)
            {
                if (track.Title.IndexOf(title, StringComparison.OrdinalIgnoreCase) > -1 &&
                    track.Artist != null && !string.IsNullOrEmpty(track.Artist.Name) &&
                    LevenshteinDistance.Compute(artist.ToLower(), track.Artist.Name.ToLower()) <=
                    Math.Abs(artist.Length - track.Artist.Name.Length))
                {
                    result = track;
                    break;
                }
            }

            Debug.Print($"Search track in local collection: {sw.ElapsedMilliseconds} ms");

            if (result != null)
                return result;

            return
                await
                    MusicStreamingPluginManager.DefaultMusicStreaming.MusicStreamingService.GetTrack(
                        $"{artist} - {title}");
        }

        public async Task<IPlayable> SearchTrack(Artist artist, string title)
        {
            PlayableBase result = null;
            var sw = Stopwatch.StartNew();
            foreach (var track in Tracks.Tracks.Where(x => x.Artist == artist))
            {
                if (track.Title.IndexOf(title, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    result = track;
                    break;
                }
            }

            Debug.Print($"Search track in local collection: {sw.ElapsedMilliseconds} ms");

            if (result != null)
                return result;

            return
                await
                    MusicStreamingPluginManager.DefaultMusicStreaming.MusicStreamingService.GetTrack(
                        $"{artist} - {title}");
        }

        public Artist SearchArtist(string name)
        {
            foreach (var artist in Artists.ArtistDictionary)
            {
                if (artist.Value.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return artist.Value;
                }
            }

            return null;
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

        private void MusicManager_TrackChanged(object sender, TrackChangedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() => UserData.UserData.History.AddEntry(e.Track, e.TimePlayed)));
        }
    }
}