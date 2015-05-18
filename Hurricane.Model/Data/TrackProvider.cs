using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Data
{
    class TrackProvider
    {
        private TrackProvider()
        {
            
        }

        public Dictionary<Guid, Playable> Collection { get; set; }

        public static TrackProvider Load(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var serializer = new XmlSerializer(typeof(Track[]));
                var result = new TrackProvider();
                var collection = new Dictionary<Guid, Playable>();
                // ReSharper disable once LoopCanBeConvertedToQuery - because of the better performance
                foreach (var track in (Track[]) serializer.Deserialize(fileStream))
                    collection.Add(track.Id, track.Playable);
                
                result.Collection = collection;
                return result;
            }
        }

        public void Save(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var serializer = new XmlSerializer(typeof(Track[]));
                serializer.Serialize(fileStream,
                    Collection.Select(x => new Track {Id = x.Key, Playable = x.Value}).ToArray());
            }
        }

        [Serializable]
        public class Track
        {
            [XmlAttribute]
            public Guid Id { get; set; }

            public Playable Playable { get; set; }
        }
    }
}