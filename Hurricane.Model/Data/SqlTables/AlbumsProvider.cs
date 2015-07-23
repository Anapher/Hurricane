using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Hurricane.Model.Music.TrackProperties;

namespace Hurricane.Model.Data.SqlTables
{
    public class AlbumsProvider : IDataProvider
    {
        private readonly ArtistProvider _artistProvider;
        private SQLiteConnection _connection;

        public AlbumsProvider(ArtistProvider artistProvider)
        {
            _artistProvider = artistProvider;
            Collection = new Dictionary<Guid, Album>();
        }

        public Dictionary<Guid, Album> Collection { get; }

        public Task CreateTables(SQLiteConnection connection)
        {
            using (
                var command =
                    new SQLiteCommand(
                        "CREATE TABLE `Albums` (Name VARCHAR(255), Guid VARCHAR(36) NOT NULL, Artists VARCHAR(8192),PRIMARY KEY (Guid))",
                        connection))
                return command.ExecuteNonQueryAsync();
        }

        public async Task Load(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand("SELECT * FROM `Albums`", connection))
            {
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var album = new Album
                    {
                        Name = reader.GetString(0),
                        Guid = reader.ReadGuid(1),
                        Artists =
                            new ObservableCollection<Artist>(
                                reader.GetString(2)?
                                    .Split( ',')
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .Select(x => _artistProvider.ArtistDictionary[Guid.ParseExact(x, "D")]) ??
                                new List<Artist>())
                    };

                    Collection.Add(album.Guid, album);
                }
            }

            _connection = connection;
        }

        public Task AddAlbum(Album album)
        {
            Collection.Add(album.Guid, album);

            using (
                var command =
                    new SQLiteCommand(
                        "INSERT INTO `Albums` (Name, Guid, Artists) VALUES (@name, @guid, @artists)",
                        _connection))
            {
                command.Parameters.AddWithValue("@name", album.Name);
                command.Parameters.AddGuid("@guid", album.Guid);
                command.Parameters.AddWithValue("@artists",
                    string.Join(",", album.Artists.Select(x => x.Guid.ToString("D"))));

                return command.ExecuteNonQueryAsync();
            }
        }

        public Task UpdateAlbumArtists(Album album)
        {
            using (var command = new SQLiteCommand("UPDATE `Albums` SET Artists=@artists WHERE Guid=@guid", _connection))
            {
                command.Parameters.AddWithValue("@artists",
                    string.Join(",", album.Artists.Select(x => x.Guid.ToString("D"))));
                command.Parameters.AddGuid("@guid", album.Guid);

                return command.ExecuteNonQueryAsync();
            }
        }
    }
}