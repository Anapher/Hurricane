using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Hurricane.Model.Music.TrackProperties;

namespace Hurricane.Model.Data
{
    public class AlbumsProvider
    {
        public AlbumsProvider()
        {
            AlbumDicitionary = new Dictionary<Guid, Album>();
        }

        public Dictionary<Guid, Album> AlbumDicitionary { get; }

        public async Task LoadFromFile(string path)
        {
            AlbumDicitionary.Clear();
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var serializer = new XmlSerializer(typeof (Album[]));
                // ReSharper disable once AccessToDisposedClosure
                foreach (var album in (Album[]) await Task.Run(() => serializer.Deserialize(fileStream)))
                    AlbumDicitionary.Add(album.Guid, album);
            }
        }

        public void SaveToFile(string path)
        {
            var tempFile = Path.GetTempFileName(); //We serialize to a temp file
            try
            {
                using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    var serializer = new XmlSerializer(typeof(Album[]));
                    // ReSharper disable once AccessToDisposedClosure

                    serializer.Serialize(fs, AlbumDicitionary.Select(x => x.Value).ToArray());
                }
                File.Copy(tempFile, path, true);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}