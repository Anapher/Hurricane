using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Hurricane.Model.Music.TrackProperties;

namespace Hurricane.Model.Data
{
    public class ArtistProvider
    {
        private static readonly Guid UnkownArtistGuid = Guid.Parse("E9F17A4B-B220-498A-A4D3-B0F712715555");

        public ArtistProvider()
        {
            ArtistDictionary = new Dictionary<Guid, Artist>();
            UnkownArtist = new Artist {Guid = UnkownArtistGuid};
        }

        public Artist UnkownArtist { get; private set; }
        public Dictionary<Guid, Artist> ArtistDictionary { get; }

        public async Task LoadFromFile(string path)
        {
            ArtistDictionary.Clear();

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var serializer = new XmlSerializer(typeof (ArtistInfo[]));
                // ReSharper disable once AccessToDisposedClosure
                var collection = await Task.Run(() => (ArtistInfo[]) serializer.Deserialize(fs));
                foreach (var item in collection)
                {
                    item.Artist.Guid = item.Id;
                    ArtistDictionary.Add(item.Id, item.Artist);
                    if (item.Id == UnkownArtistGuid)
                        UnkownArtist = item.Artist;
                }
            }
        }

        public void SaveToFile(string path)
        {
            var tempFile = Path.GetTempFileName(); //We serialize to a temp file
            try
            {
                using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    var serializer = new XmlSerializer(typeof (ArtistInfo[]));
                    serializer.Serialize(fs,
                        ArtistDictionary.Select(x => new ArtistInfo {Id = x.Key, Artist = x.Value}).ToArray());
                }
                File.Copy(tempFile, path, true);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Serializable]
        public class ArtistInfo
        {
            [XmlAttribute]
            public Guid Id { get; set; }
            public Artist Artist { get; set; }
        }
    }
}
