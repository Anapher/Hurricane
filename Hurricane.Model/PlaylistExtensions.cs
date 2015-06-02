using System;
using System.Collections.Generic;
using System.Linq;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;

namespace Hurricane.Model
{
    public static class PlaylistExtensions
    {
        private static readonly Random PrivateRandom = new Random();
        private static readonly Dictionary<IPlaylist, List<IPlayable>> ShuffleHistoryDictionary = new Dictionary<IPlaylist, List<IPlayable>>();
        private static readonly Dictionary<IPlaylist, List<IPlayable>> BackHistoryDictionary = new Dictionary<IPlaylist, List<IPlayable>>();

        public static IPlayable GetNextTrack(this IPlaylist playlist, IPlayable currentTrack)
        {
            if (playlist.Tracks.Count == 1 || currentTrack == null || !playlist.Tracks.Contains(currentTrack))
                return playlist.Tracks[0];

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

        public static IPlayable GetPreviousTrack(this IPlaylist playlist, IPlayable currentTrack)
        {
            if (playlist.Tracks.Count == 1 || currentTrack == null || !playlist.Tracks.Contains(currentTrack))
                return playlist.Tracks[0];

            int tracksToCheck = playlist.Tracks.Count;
            int currentIndex = playlist.Tracks.IndexOf(currentTrack);

            while (tracksToCheck > 0)
            {
                tracksToCheck--;
                currentIndex--;

                if (currentIndex < 0)
                    currentIndex = playlist.Tracks.Count;

                if (playlist.Tracks[currentIndex].IsAvailable)
                    return playlist.Tracks[currentIndex];
            }

            return playlist.Tracks[0];
        }

        public static IPlayable GetRandomTrack(this IPlaylist playlist)
        {
            if (playlist.Tracks.Count == 1)
                return playlist.Tracks[0];
            var history = playlist.GetShuffleHistory();

            foreach (var track in history.Where(track => !playlist.Tracks.Contains(track)))
            {
                history.Remove(track); //We remove all tracks from the history which aren't in the playlist any more
            }

            var shuffleList = playlist.Tracks.Where(x => x.IsAvailable && !history.Contains(x)).ToList(); //We search all tracks which are available and not in the history
            if (shuffleList.Count == 1) //If there is only one item, we return that
                return shuffleList[0];

            if (shuffleList.Count == 0)
            {
                history.Clear();
                shuffleList.AddRange(playlist.Tracks.Where(x => x.IsAvailable));
            }

            return shuffleList[PrivateRandom.Next(shuffleList.Count)];
        }

        public static List<IPlayable> GetShuffleHistory(this IPlaylist playlist)
        {
            if (!ShuffleHistoryDictionary.ContainsKey(playlist))
                ShuffleHistoryDictionary.Add(playlist, new List<IPlayable>());

            return ShuffleHistoryDictionary[playlist];
        }

        public static List<IPlayable> GetBackHistory(this IPlaylist playlist)
        {
            if (!BackHistoryDictionary.ContainsKey(playlist))
                BackHistoryDictionary.Add(playlist, new List<IPlayable>());

            return BackHistoryDictionary[playlist];
        } 
    }
}