using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Music.Playlist
{
    public class UserPlaylist : IPlaylist
    {
        public UserPlaylist()
        {
            Tracks = new ObservableCollection<IPlayable>();
            History = new List<IPlayable>();
        }

        public IList<IPlayable> Tracks { get; }
        public IList<IPlayable> History { get; }
        public string Name { get; set; }
    }
}