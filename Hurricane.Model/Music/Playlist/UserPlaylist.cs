using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Music.Playlist
{
    public class UserPlaylist : IPlaylist
    {
        public UserPlaylist()
        {
            Tracks = new ObservableCollection<PlayableBase>();
            History = new List<IPlayable>();
        }

        public ObservableCollection<PlayableBase> Tracks { get; }
        public List<IPlayable> History { get; }
        public string Name { get; set; }

        IList<IPlayable> IPlaylist.Tracks => Tracks.Cast<IPlayable>().ToList();

        IList<IPlayable> IPlaylist.History => History;

        public void AddTrack(PlayableBase playable)
        {
            Tracks.Add(playable);
        }

        public void RemoveTrack(PlayableBase playable)
        {
            Tracks.Remove(playable);
            History.Remove(playable);
        }
    }
}