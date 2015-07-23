using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using Hurricane.Model.Music.TrackProperties;

namespace Hurricane.Model.Data.SqlTables
{
    public class ArtistProvider : IDataProvider
    {
        public static readonly Guid UnknownArtistGuid = Guid.Parse("E9F17A4B-B220-498A-A4D3-B0F712715555");
        private readonly ImagesProvider _imageProvider;
        private SQLiteConnection _connection;

        public ArtistProvider(ImagesProvider imageProvider)
        {
            ArtistDictionary = new Dictionary<Guid, Artist>();
            UnknownArtist = new Artist {Guid = UnknownArtistGuid};
            _imageProvider = imageProvider;
        }

        public Artist UnknownArtist { get; private set; }
        public Dictionary<Guid, Artist> ArtistDictionary { get; }

        public Task CreateTables(SQLiteConnection connection)
        {
            using (
                var command =
                    new SQLiteCommand(
                        "CREATE TABLE `Artists` (Name VARCHAR(255), MusicBrainzId VARCHAR(36), Url VARCHAR(255), Guid VARCHAR(36) NOT NULL, SmallImage VARCHAR(36), MediumImage VARCHAR(36), LargeImage VARCHAR(36), PRIMARY KEY (Guid))",
                        connection))
                return command.ExecuteNonQueryAsync();
        }

        public async Task Load(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand("SELECT * FROM `Artists`", connection))
            {
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var id = reader.ReadGuid(3);
                    Artist artist;
                    if (id == UnknownArtistGuid)
                        artist = UnknownArtist;
                    else
                    {
                        artist = new Artist
                        {
                            Name = reader.GetString(0),
                            MusicBrainzId = reader.GetValue(1)?.ToString(),
                            Url = reader.GetValue(2)?.ToString(),
                            Guid = id
                        };
                    }

                    var temp = reader.ReadGuid(4);
                    if (temp != Guid.Empty)
                        artist.SmallImage = _imageProvider.Collection[temp];
                    temp = reader.ReadGuid(5);
                    if (temp != Guid.Empty)
                        artist.MediumImage = _imageProvider.Collection[temp];
                    temp = reader.ReadGuid(6);
                    if (temp != Guid.Empty)
                        artist.LargeImage = _imageProvider.Collection[temp];

                    ArtistDictionary.Add(artist.Guid, artist);
                    if (artist.Guid == UnknownArtistGuid)
                        UnknownArtist = artist;
                }
            }

            _connection = connection;
        }

        public Task AddArtist(Artist artist)
        {
            ArtistDictionary.Add(artist.Guid, artist);

            _imageProvider.AddImage(artist.SmallImage);
            _imageProvider.AddImage(artist.MediumImage);
            _imageProvider.AddImage(artist.LargeImage);

            using (
                var command =
                    new SQLiteCommand(
                        "INSERT INTO `Artists` (Name, MusicBrainzId, Url, Guid, SmallImage, MediumImage, LargeImage) VALUES (@name, @musicBrainzId, @url, @guid, @smallImage, @mediumImage, @largeImage)",
                        _connection))
            {
                command.Parameters.AddWithValue("@name", artist.Name);
                command.Parameters.AddWithValue("@musicBrainzId", artist.MusicBrainzId);
                command.Parameters.AddWithValue("@url", artist.Url);
                command.Parameters.AddGuid("@guid", artist.Guid);
                command.Parameters.AddGuid("@smallImage", artist.SmallImage?.Guid);
                command.Parameters.AddGuid("@mediumImage", artist.MediumImage?.Guid);
                command.Parameters.AddGuid("@largeImage", artist.LargeImage?.Guid);

                return command.ExecuteNonQueryAsync();
            }
        }
    }
}
