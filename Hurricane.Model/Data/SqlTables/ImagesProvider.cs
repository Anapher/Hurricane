using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using Hurricane.Model.Music.Imagment;
using TaskExtensions = Hurricane.Utilities.TaskExtensions;

namespace Hurricane.Model.Data.SqlTables
{
    public class ImagesProvider : IDataProvider
    {
        private SQLiteConnection _connection;

        public ImagesProvider()
        {
            Collection = new Dictionary<Guid, ImageProvider>();
        }

        public Dictionary<Guid, ImageProvider> Collection { get; }
        
        public Task CreateTables(SQLiteConnection connection)
        {
            using (
                var command =
                    new SQLiteCommand("CREATE TABLE `Images` (Type INT, Guid VARCHAR(36) NOT NULL, Data VARCHAR(400), PRIMARY KEY (Guid))",
                        connection))
                return command.ExecuteNonQueryAsync();
        }

        public async Task Load(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand("SELECT * FROM `Images`", connection))
            {
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var id = reader.ReadGuid(1);
                    switch (reader.GetInt32(0))
                    {
                        case 0:
                            Collection.Add(id, new TagImage(reader.GetString(2)));
                            break;
                        case 1:
                            Collection.Add(id, new OnlineImage(reader.GetString(2)));
                            break;
                    }
                }
            }

            _connection = connection;
        }

        public Task AddImage(TagImage image)
        {
            if (Collection.ContainsKey(image.Guid))
                return TaskExtensions.CompletedTask;

            return AddImageRow(image, 0, image.FilePath);
        }

        public Task AddImage(OnlineImage image)
        {
            if (Collection.ContainsKey(image.Guid))
                return TaskExtensions.CompletedTask;

            return AddImageRow(image, 1, image.Url);
        }

        public Task AddImage(BitmapImageProvider image)
        {
            throw new NotSupportedException();
        }

        public Task AddImage(ImageProvider image)
        {
            if (image == null)
                return TaskExtensions.CompletedTask;

            var onlineImage = image as OnlineImage;
            if (onlineImage != null)
                return AddImage(onlineImage);

            var tagImage = image as TagImage;
            if (tagImage != null)
                return AddImage(tagImage);

            throw new ArgumentException(nameof(image));
        }

        private Task AddImageRow(ImageProvider image, int id, string data)
        {
            Collection.Add(image.Guid, image);

            using (
                var command =
                    new SQLiteCommand(
                        "INSERT INTO `Images` (Type, Guid, Data) VALUES (@type, @guid, @data)",
                        _connection))
            {
                command.Parameters.AddWithValue("@type", id);
                command.Parameters.AddGuid("@guid", image.Guid);
                command.Parameters.AddWithValue("@data", data);

                return command.ExecuteNonQueryAsync();
            }
        }
    }
}
