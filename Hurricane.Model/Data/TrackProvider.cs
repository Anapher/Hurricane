using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Data
{
    public class TrackProvider
    {
        private readonly XmlSerializer _serializer;
        public TrackProvider()
        {
            Collection = new Dictionary<Guid, PlayableBase>();

            var aor = new XmlAttributeOverrides();
            _serializer = new XmlSerializer(typeof(Track[]), new[] {typeof(LocalPlayable)});
        }

        public Dictionary<Guid, PlayableBase> Collection { get; set; }

        public async Task LoadFromFile(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var collection = new Dictionary<Guid, PlayableBase>();
                // ReSharper disable once LoopCanBeConvertedToQuery - because of the better performance
                // ReSharper disable once AccessToDisposedClosure
                foreach (var track in (Track[]) await Task.Run(() => _serializer.Deserialize(fileStream)))
                    collection.Add(track.Id, track.Playable);

                Collection = collection;
            }
        }

        public void SaveToFile(string path)
        {
            var tempFile = Path.GetTempFileName(); //We serialize to a temp file
            try
            {
                using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // ReSharper disable once AccessToDisposedClosure
                    _serializer.Serialize(fileStream,
                        Collection.Select(x => new Track {Id = x.Key, Playable = x.Value}).ToArray());
                }
                File.Copy(tempFile, path, true);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public void LoadData(ArtistProvider artistProvider)
        {
            foreach (var playableBase in Collection)
            {
                playableBase.Value.Artist = artistProvider.ArtistDictionary[playableBase.Value.ArtistGuid];
            }
        }

        [Serializable, XmlType(TypeName = "Playable")]
        public class Track
        {
            [XmlAttribute]
            public Guid Id { get; set; }

            public PlayableBase Playable { get; set; }
        }
    }
}