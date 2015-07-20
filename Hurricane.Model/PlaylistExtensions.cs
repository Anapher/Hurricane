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
        private static readonly Dictionary<object, List<IPlayable>> ShuffleHistoryDictionary = new Dictionary<object, List<IPlayable>>();
        private static readonly Dictionary<object, List<IPlayable>> BackHistoryDictionary = new Dictionary<object, List<IPlayable>>();

        public static IPlayable GetNextTrack<T>(this IList<T> tracks, IPlayable currentTrack) where T:IPlayable
        {
            if (tracks.Count == 1 || currentTrack == null || !tracks.Any(x => x.Equals(currentTrack)))
                return tracks[0];

            int tracksToCheck = tracks.Count;
            int currentIndex = tracks.IndexOf(currentTrack);
            while (tracksToCheck > 0)
            {
                tracksToCheck--;
                currentIndex += 1;
                if (currentIndex > tracks.Count - 1)
                    currentIndex = 0;

                if (tracks[currentIndex].IsAvailable)
                    return tracks[currentIndex];
            }

            return tracks[0];
        }

        public static IPlayable GetPreviousTrack<T>(this IList<T> tracks, IPlayable currentTrack) where T : IPlayable
        {
            if (tracks.Count == 1 || currentTrack == null || !tracks.Any(x => x.Equals(currentTrack)))
                return tracks[0];

            int tracksToCheck = tracks.Count;
            int currentIndex = tracks.IndexOf(currentTrack);

            while (tracksToCheck > 0)
            {
                tracksToCheck--;
                currentIndex--;

                if (currentIndex < 0)
                    currentIndex = tracks.Count;

                if (tracks[currentIndex].IsAvailable)
                    return tracks[currentIndex];
            }

            return tracks[0];
        }

        public static IPlayable GetRandomTrack<T>(this IList<T> tracks) where T : IPlayable
        {
            if (tracks.Count == 1)
                return tracks[0];
            var history = tracks.GetShuffleHistory();

            foreach (var track in history.Where(track => !tracks.Any(x => x.Equals(track))))
            {
                history.Remove(track); //We remove all tracks from the history which aren't in the playlist any more
            }

            var shuffleList = tracks.Where(x => x.IsAvailable && !history.Contains(x)).ToList(); //We search all tracks which are available and not in the history
            if (shuffleList.Count == 1) //If there is only one item, we return that
                return shuffleList[0];

            if (shuffleList.Count == 0)
            {
                history.Clear();
                shuffleList.AddRange(tracks.Where(x => x.IsAvailable));
            }

            return shuffleList[PrivateRandom.Next(shuffleList.Count)];
        }

        public static List<IPlayable> GetShuffleHistory(this object obj)
        {
            if (!ShuffleHistoryDictionary.ContainsKey(obj))
                ShuffleHistoryDictionary.Add(obj, new List<IPlayable>());

            return ShuffleHistoryDictionary[obj];
        }

        public static List<IPlayable> GetBackHistory(this IPlaylist playlist)
        {
            if (!BackHistoryDictionary.ContainsKey(playlist))
                BackHistoryDictionary.Add(playlist, new List<IPlayable>());

            return BackHistoryDictionary[playlist];
        }

        public static int IndexOf<T>(this IList<T> source, object obj)
        {
            for (var i = 0; i < source.Count; i++)
            {
                if (obj.Equals(source[i]))
                    return i;
            }
            return -1;
        }
    }
}