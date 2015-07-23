using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Hurricane.Model.Data.SqlTables;
using Hurricane.Model.DataApi;
using Hurricane.Model.Music;
using Hurricane.Model.Music.Args;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.Model.Plugins.MusicStreaming;
using Hurricane.Model.Settings;
using Hurricane.Utilities;

namespace Hurricane.Model.Data
{
    public class MusicDataManager : IDisposable
    {
        private const string UserDataFilename = "UserData.xml";
        private readonly FileInfo _databaseFile;
        private SQLiteConnection _connection;

        public MusicDataManager()
        {
            _databaseFile =
                new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Hurricane", "database.sqlite"));

            Images = new ImagesProvider();
            Artists = new ArtistProvider(Images);
            Albums = new AlbumsProvider(Artists);
            Tracks = new TrackProvider(Artists, Images, Albums);
            Playlists = new PlaylistProvider(Tracks);
            UserData = new UserDataProvider();

            LastfmApi = new LastfmApi(Artists);
            MusicManager = new MusicManager();
            MusicManager.TrackChanged += MusicManager_TrackChanged;
            MusicManager.NewTrackOpened += MusicManager_NewTrackOpened;
            MusicStreamingPluginManager = new MusicStreamingPluginManager();
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
            MusicManager.Dispose();
        }

        public ImagesProvider Images { get; set; }
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
            var createTables = false;

            if (!_databaseFile.Exists)
            {
                SQLiteConnection.CreateFile(_databaseFile.FullName);
                createTables = true;
            }

            var dataProvider = new IDataProvider[] {Images, Artists, Albums, Tracks, Playlists};

            _connection = new SQLiteConnection($"Data Source={_databaseFile.FullName};Version=3;");
            await _connection.OpenAsync();

            if(createTables)
                foreach (var provider in dataProvider)
                    await provider.CreateTables(_connection);

            foreach (var data in dataProvider)
                await data.Load(_connection);
           
            var userDataFileInfo = new FileInfo(Path.Combine(rootFolder, UserDataFilename));

            if (userDataFileInfo.Exists)
                await UserData.LoadFromFile(userDataFileInfo.FullName);

            LoadSettings();
        }

        public void Save(string rootFolder)
        {
            if (MusicManager.CurrentTrack != null)
                UserData.UserData.History.AddEntry(MusicManager.CurrentTrack,
                    MusicManager.AudioEngine.TimePlaySourcePlayed);

            UserData.SaveToFile(Path.Combine(rootFolder, UserDataFilename));

            CopyToSettings();
        }

        public async Task<IPlayable> SearchTrack(string artist, string title)
        {
            PlayableBase result = null;
            var sw = Stopwatch.StartNew();
            foreach (var track in Tracks.Tracks)
            {
                if (track.Title.IndexOf(title, StringComparison.OrdinalIgnoreCase) > -1 && !string.IsNullOrEmpty(track.Artist?.Name) && LevenshteinDistance.Compute(artist.ToLower(), track.Artist.Name.ToLower()) <=
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
                    MusicStreamingPluginManager.DefaultMusicStreaming.GetTrack(
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
                    MusicStreamingPluginManager.DefaultMusicStreaming.GetTrack(
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

        private void MusicManager_NewTrackOpened(object sender, NewTrackOpenedEventArgs e)
        {
            var track = e.NewTrack as PlayableBase;
            if (track != null)
                Tracks.UpdateLastTimePlayed(track, DateTime.Now);
        }
    }
}