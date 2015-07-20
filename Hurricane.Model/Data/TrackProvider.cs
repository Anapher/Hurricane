using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;

namespace Hurricane.Model.Data
{
    public class TrackProvider : IPlaylist
    {
        public static readonly Guid Id = new Guid("D6209B00-17F6-40EB-8634-B96BE432740D");
        private readonly XmlSerializer _serializer;
        public TrackProvider()
        {
            Collection = new Dictionary<Guid, PlayableBase>();
            Tracks = new ObservableCollection<PlayableBase>();
            _serializer = new XmlSerializer(typeof(Track[]), new[] {typeof(LocalPlayable)});
        }

        public Dictionary<Guid, PlayableBase> Collection { get; set; }
        public ObservableCollection<PlayableBase> Tracks { get; set; }

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
                Tracks = new ObservableCollection<PlayableBase>(Collection.Select(x => x.Value));
            }
        }

        public void SaveToFile(string path)
        {
            var tempFile = Path.GetTempFileName(); //We serialize to a temp file
            try
            {
                using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
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

        public void LoadData(ArtistProvider artistProvider, AlbumsProvider albumsProvider)
        {
            foreach (var playableBase in Collection)
            {
                playableBase.Value.Artist = artistProvider.ArtistDictionary[playableBase.Value.ArtistGuid];
                if (playableBase.Value.AlbumGuid != Guid.Empty)
                    playableBase.Value.Album = albumsProvider.AlbumDicitionary[playableBase.Value.AlbumGuid];
            }
        }

        public void AddTrack(PlayableBase track)
        {
            Collection.Add(Guid.NewGuid(), track);
            Tracks.Add(track);
        }

        public void RemoveTrack(PlayableBase track)
        {
            Collection.Remove(Collection.First(x => x.Value == track).Key);
            Tracks.Remove(track);
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

        [Serializable, XmlType(TypeName = "Playable")]
        public class Track
        {
            [XmlAttribute]
            public Guid Id { get; set; }

            public PlayableBase Playable { get; set; }
        }
    }
}