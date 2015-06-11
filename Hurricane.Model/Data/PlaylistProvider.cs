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
    public class PlaylistProvider
    {
        public PlaylistProvider()
        {
            Playlists = new ObservableCollection<UserPlaylist>();
        }

        public event EventHandler<UserPlaylist> PlaylistAdded;
        public event EventHandler<UserPlaylist> PlaylistRemoved;

        public ObservableCollection<UserPlaylist> Playlists { get; }

        public void AddPlaylist(UserPlaylist playlist)
        {
            Playlists.Add(playlist);
            PlaylistAdded?.Invoke(this, playlist);
        }

        public void RemovePlaylist(UserPlaylist playlist)
        {
            Playlists.Remove(playlist);
            PlaylistRemoved?.Invoke(this, playlist);
        }

        public async Task LoadFromFile(string path, Dictionary<Guid, PlayableBase> tracks)
        {
            Playlists.Clear();
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var serializer = new XmlSerializer(typeof (PlaylistInfo[]));
                // ReSharper disable once AccessToDisposedClosure
                foreach (var playlist in (PlaylistInfo[]) await Task.Run(() => serializer.Deserialize(fileStream)))
                {
                    var userPlaylist = new UserPlaylist {Name = playlist.Name};
                    foreach (var playable in playlist.Playables)
                        userPlaylist.Tracks.Add(tracks[playable]);
                    Playlists.Add(userPlaylist);
                }
            }
        }

        public void SaveToFile(string path, Dictionary<Guid, PlayableBase> tracks)
        {
            var tempFile = Path.GetTempFileName(); //We serialize to a temp file
            try
            {
                using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    var serializer = new XmlSerializer(typeof (PlaylistInfo[]));
                    serializer.Serialize(fs,
                        Playlists.Select(userPlaylist => new PlaylistInfo
                        {
                            Name = userPlaylist.Name,
                            Playables =
                                userPlaylist.Tracks.Select(x => tracks.First(y => y.Value == x).Key)
                                    .ToArray()
                        }).ToArray());
                }
                File.Copy(tempFile, path, true);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Serializable, XmlRoot("Playlists")]
        public class PlaylistInfo
        {
            [XmlAttribute]
            public string Name { get; set; }
            public Guid[] Playables { get; set; }
        }
    }
}
