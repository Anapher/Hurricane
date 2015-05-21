using System;
using System.Linq;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;

namespace Hurricane.Model
{
    public static class PlaylistExtensions
    {
        private static readonly Random PrivateRandom = new Random();

        public static IPlayable GetNextTrack(this IPlaylist playlist, IPlayable currentTrack)
        {
            if (playlist.Tracks.Count == 1 || currentTrack == null || !playlist.Tracks.Contains(currentTrack))
                return playlist.Tracks.First();

            int tracksToCheck = playlist.Tracks.Count;
            int currentIndex = playlist.Tracks.IndexOf(currentTrack);
            while (tracksToCheck > 0)
            {
                tracksToCheck--;
                currentIndex += 1;
                if (currentIndex > playlist.Tracks.Count - 1)
                    currentIndex = 0;

                if (playlist.Tracks[currentIndex].IsAvailable)
                    return playlist.Tracks[currentIndex];
            }

            return playlist.Tracks[0];
        }

        public static IPlayable GetRandomTrack(this IPlaylist playlist)
        {
            if (playlist.Tracks.Count == 0) return playlist.Tracks[0];
            foreach (var track in playlist.History)
            {
                if (!playlist.Tracks.Contains(track))
                    playlist.History.Remove(track);
            }

            var shuffleList = playlist.Tracks.Where(x => x.IsAvailable && !playlist.History.Contains(x)).ToList();
            if (shuffleList.Count == 1)
                return shuffleList[0];

            if (shuffleList.Count == 0)
            {
                playlist.History.Clear();
                shuffleList.AddRange(playlist.Tracks.Where(x => x.IsAvailable));
            }

            return shuffleList[PrivateRandom.Next(shuffleList.Count)];
        }
    }
}