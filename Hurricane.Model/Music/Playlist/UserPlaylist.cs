using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Music.Playlist
{
    public class UserPlaylist : IPlaylist
    {
        public UserPlaylist()
        {
            Tracks = new ObservableCollection<PlayableBase>();
            History = new List<IPlayable>();
            Id = Guid.NewGuid();
        }

        public event EventHandler<PlayableBase> TrackAdded;
        public event EventHandler<PlayableBase> TrackRemoved;

        public ObservableCollection<PlayableBase> Tracks { get; }
        public List<IPlayable> History { get; }
        public string Name { get; set; }
        public Guid Id { get; set; }

        public void AddTrack(PlayableBase playable)
        {
            Tracks.Add(playable);
            TrackAdded?.Invoke(this, playable);
        }
        
        public void RemoveTrack(PlayableBase playable)
        {
            Tracks.Remove(playable);
            History.Remove(playable);
            TrackRemoved?.Invoke(this, playable);
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

        public Task<IPlayable> GetLastTrack()
        {
            return Task.FromResult((IPlayable) Tracks.Last());
        }

        bool IPlaylist.ContainsPlayableTracks()
        {
            return Tracks.Count > 0 && Tracks.Any(x => x.IsAvailable);
        }
    }
}