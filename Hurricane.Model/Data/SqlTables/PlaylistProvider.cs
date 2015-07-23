using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Threading.Tasks;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;

namespace Hurricane.Model.Data.SqlTables
{
    public class PlaylistProvider : IDataProvider
    {
        private readonly TrackProvider _trackProvider;
        private SQLiteConnection _connection;

        public PlaylistProvider(TrackProvider trackProvider)
        {
            _trackProvider = trackProvider;
            Playlists = new ObservableCollection<UserPlaylist>();
        }

        public event EventHandler<UserPlaylist> PlaylistAdded;
        public event EventHandler<UserPlaylist> PlaylistRemoved;

        public ObservableCollection<UserPlaylist> Playlists { get; }

        public async Task CreateTables(SQLiteConnection connection)
        {
            using (
                var command =
                    new SQLiteCommand(
                        "CREATE TABLE `Playlists` (Name VARCHAR(255), Guid VARCHAR(36) NOT NULL, PRIMARY KEY (Guid))",
                        connection))
                await command.ExecuteNonQueryAsync();

            using (
                var command =
                    new SQLiteCommand(
                        "CREATE TABLE `PlaylistTracks` (PlaylistId VARCHAR(36), TrackId VARCHAR(36))",
                        connection))
                await command.ExecuteNonQueryAsync();

            using (
                var command =
                    new SQLiteCommand(
                        "INSERT INTO `Playlists` (Name, Guid) VALUES (@name, @guid)", connection))
            {
                command.Parameters.AddWithValue("@name", "Default Playlist");
                command.Parameters.AddGuid("@guid", Guid.NewGuid());

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task Load(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand("SELECT * FROM `Playlists`", connection))
            {
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var playlist = new UserPlaylist {Name = reader.GetString(0), Id = reader.GetGuid(1)};
                    using (var trackCommando = new SQLiteCommand("SELECT * FROM `PlaylistTracks` WHERE PlaylistId=@guid", connection ))
                    {
                        trackCommando.Parameters.AddGuid("@guid", playlist.Id);
                        var trackReader = trackCommando.ExecuteReader();
                        while (await trackReader.ReadAsync())
                            playlist.Tracks.Add(_trackProvider.Collection[trackReader.GetGuid(1)]);
                    }
                    playlist.TrackAdded += Playlist_TrackAdded;
                    playlist.TrackRemoved += Playlist_TrackRemoved;

                    Playlists.Add(playlist);
                }
            }

            _connection = connection;
        }

        private async void Playlist_TrackAdded(object sender, PlayableBase e)
        {
            using (
                var command =
                    new SQLiteCommand(
                        "INSERT INTO `PlaylistTracks` (PlaylistId, TrackId) VALUES (@playlistId, @trackId)", _connection)
                )
            {
                command.Parameters.AddGuid("@playlistId", ((UserPlaylist) sender).Id);
                command.Parameters.AddGuid("@trackId", e.Guid);

                await command.ExecuteNonQueryAsync();
            }
        }

        private async void Playlist_TrackRemoved(object sender, PlayableBase e)
        {
            using (
                var command =
                    new SQLiteCommand(
                        "DELETE FROM `PlaylistTracks` WHERE PlaylistId=@playlistId AND TrackId=@trackId", _connection)
                )
            {
                command.Parameters.AddGuid("@playlistId", ((UserPlaylist) sender).Id);
                command.Parameters.AddGuid("@trackId", e.Guid);

                await command.ExecuteNonQueryAsync();
            }
        }

        public Task AddPlaylist(UserPlaylist playlist)
        {
            Playlists.Add(playlist);
            PlaylistAdded?.Invoke(this, playlist);

            using (
                var command =
                    new SQLiteCommand(
                        "INSERT INTO `Playlists` (Name, Guid) VALUES (@name, @guid)", _connection))
            {
                command.Parameters.AddWithValue("@name", playlist.Name);
                command.Parameters.AddGuid("@guid", playlist.Id);

                return command.ExecuteNonQueryAsync();
            }
        }

        public async Task RemovePlaylist(UserPlaylist playlist)
        {
            Playlists.Remove(playlist);
            PlaylistRemoved?.Invoke(this, playlist);

            using (
                var command =
                    new SQLiteCommand(
                        "DELETE FROM `Playlists` WHERE Guid=@guid", _connection))
            {
                command.Parameters.AddGuid("@guid", playlist.Id);

                await command.ExecuteNonQueryAsync();
            }

            using (
                var command =
                    new SQLiteCommand(
                        "DELETE FROM `PlaylistTracks` WHERE PlaylistId=@guid", _connection))
            {
                command.Parameters.AddGuid("@guid", playlist.Id);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
