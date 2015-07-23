using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;

namespace Hurricane.Model.Data.SqlTables
{
    public class TrackProvider : IPlaylist, IDataProvider
    {
        public static readonly Guid Id = new Guid("D6209B00-17F6-40EB-8634-B96BE432740D");
        private readonly XmlSerializer _serializer;
        private readonly ArtistProvider _artistProvider;
        private readonly ImagesProvider _imageProvider;
        private readonly AlbumsProvider _albumsProvider;

        private SQLiteConnection _connection;

        public TrackProvider(ArtistProvider artistProvider, ImagesProvider imageProvider, AlbumsProvider albumsProvider)
        {
            Collection = new Dictionary<Guid, PlayableBase>();
            Tracks = new ObservableCollection<PlayableBase>();
            _serializer = new XmlSerializer(typeof(LocalPlayable));
            _artistProvider = artistProvider;
            _imageProvider = imageProvider;
            _albumsProvider = albumsProvider;
        }

        public Dictionary<Guid, PlayableBase> Collection { get; set; }
        public ObservableCollection<PlayableBase> Tracks { get; set; }

        public Task CreateTables(SQLiteConnection connection)
        {
            using (
                var command =
                    new SQLiteCommand(
                        "CREATE TABLE `Tracks` (Title VARCHAR(255), ArtistGuid VARCHAR(36), AlbumGuid VARCHAR(36), Guid VARCHAR(36) NOT NULL, LastTimePlayed DATETIME, MusicBrainzId VARCHAR(36), Duration VARCHAR(25), Cover VARCHAR(36), XmlData VARCHAR(1024), PRIMARY KEY (Guid))",
                        connection))
                return command.ExecuteNonQueryAsync();
        }

        public async Task Load(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand("SELECT * FROM `Tracks`", connection))
            {
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    PlayableBase track;
                    using (var xmlReader = reader.GetTextReader(8))
                        track = (PlayableBase) _serializer.Deserialize(xmlReader);

                    track.Title = reader.GetString(0);
                    track.Artist = _artistProvider.ArtistDictionary[reader.ReadGuid(1)];

                    var albumGuid = reader.ReadGuid(2);
                    if (albumGuid != Guid.Empty)
                        track.Album = _albumsProvider.Collection[albumGuid];
                    track.Guid = reader.ReadGuid(3);
                    track.LastTimePlayed = reader.GetDateTime(4);
                    track.MusicBrainzId = reader.GetValue(5)?.ToString();
                    track.Duration = XmlConvert.ToTimeSpan(reader.GetString(6));

                    var coverId = reader.ReadGuid(7);
                    if (coverId != Guid.Empty)
                        track.Cover = _imageProvider.Collection[coverId];

                    Collection.Add(track.Guid, track);
                    Tracks.Add(track);
                }
            }

            _connection = connection;
        }

        public Task AddTrack(PlayableBase track)
        {
            track.Guid = Guid.NewGuid();
            Collection.Add(track.Guid, track);
            Tracks.Add(track);
            _imageProvider.AddImage(track.Cover);

            using (
                var command =
                    new SQLiteCommand(
                        "INSERT INTO `Tracks` (Title, ArtistGuid, AlbumGuid, Guid, LastTimePlayed, MusicBrainzId, Duration, Cover, XmlData) VALUES (@title, @artistGuid, @albumGuid, @guid, @lastTimePlayed, @musicBrainzId, @duration, @cover, @xmlData)",
                        _connection))
            {
                command.Parameters.AddWithValue("@title", track.Title);
                command.Parameters.AddGuid("@artistGuid", track.Artist.Guid);
                command.Parameters.AddGuid("@albumGuid", track.Album?.Guid);
                command.Parameters.AddGuid("@guid", track.Guid);
                command.Parameters.AddWithValue("@lastTimePlayed", track.LastTimePlayed.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@musicBrainzId", track.MusicBrainzId);
                command.Parameters.AddWithValue("@duration", XmlConvert.ToString(track.Duration));
                command.Parameters.AddGuid("@cover", track.Cover?.Guid);
                using (var stringWriter = new StringWriter())
                {
                    _serializer.Serialize(stringWriter, track);
                    command.Parameters.AddWithValue("@xmlData", stringWriter.ToString());
                }

                return command.ExecuteNonQueryAsync();
            }
        }

        public Task RemoveTrack(PlayableBase track)
        {
            Collection.Remove(track.Guid);
            Tracks.Remove(track);

            using (var command = new SQLiteCommand("DELETE FROM `Tracks` WHERE Guid=@guid", _connection))
            {
                command.Parameters.AddGuid("@guid", track.Guid);
                return command.ExecuteNonQueryAsync();
            }
        }

        public Task UpdateLastTimePlayed(PlayableBase track, DateTime newLastTimePlayed)
        {
            track.LastTimePlayed = newLastTimePlayed;
            using (var command = new SQLiteCommand("UPDATE `Tracks` SET LastTimePlayed=@newLastTimePlayed WHERE Guid=@guid", _connection))
            {
                command.Parameters.AddWithValue("@newLastTimePlayed", newLastTimePlayed.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddGuid("@guid", track.Guid);

                return command.ExecuteNonQueryAsync();
            }
        }

        Task<IPlayable> IPlaylist.GetNextTrack(IPlayable currentTrack)
        {
            return Task.FromResult(Tracks.GetNextTrack(currentTrack));
        }

        Task<IPlayable> IPlaylist.GetShuffleTrack()
        {
            return Task.FromResult(Tracks.GetRandomTrack());
        }

        Task<IPlayable> IPlaylist.GetPreviousTrack(IPlayable currentTrack)
        {
            return Task.FromResult(Tracks.GetPreviousTrack(currentTrack));
        }

        Task<IPlayable> IPlaylist.GetLastTrack()
        {
            return Task.FromResult((IPlayable) Tracks.Last());
        }

        bool IPlaylist.ContainsPlayableTracks()
        {
            return Tracks.Count > 0 && Tracks.Any(x => x.IsAvailable);
        }
    }
}