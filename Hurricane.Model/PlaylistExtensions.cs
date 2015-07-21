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
        private static readonly Dictionary<object, List<object>> ShuffleHistoryDictionary = new Dictionary<object, List<object>>();
        private static readonly Dictionary<object, List<object>> BackHistoryDictionary = new Dictionary<object, List<object>>();

        /// <summary>
        /// Calculates the next, available track relative to <see cref="currentTrack"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tracks">The list of the tracks</param>
        /// <param name="currentTrack">The current track</param>
        /// <returns>Returns the next track, relative to <see cref="currentTrack"/></returns>
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

        /// <summary>
        /// Calculates the previous, available track relative to <see cref="currentTrack"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tracks">The list of the tracks</param>
        /// <param name="currentTrack">The current track</param>
        /// <returns>Returns the previous track, relative to <see cref="currentTrack"/></returns>
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

        /// <summary>
        /// Gets a random, available track. It uses a history to prevent repeating
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tracks">The list of the track</param>
        /// <returns>A random track</returns>
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

        /// <summary>
        /// Returns the shuffle history of the list
        /// </summary>
        /// <param name="obj">The list</param>
        /// <returns></returns>
        public static List<object> GetShuffleHistory(this object obj)
        {
            if (!ShuffleHistoryDictionary.ContainsKey(obj))
                ShuffleHistoryDictionary.Add(obj, new List<object>());

            return ShuffleHistoryDictionary[obj];
        }

        /// <summary>
        /// Returns the history for going back of the list
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public static List<object> GetBackHistory(this IPlaylist playlist)
        {
            if (!BackHistoryDictionary.ContainsKey(playlist))
                BackHistoryDictionary.Add(playlist, new List<object>());

            return BackHistoryDictionary[playlist];
        }

        /// <summary>
        /// Returns the index of the <see cref="obj"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this IList<T> source, object obj)
        {
            for (var i = 0; i < source.Count; i++)
            {
                if (obj.Equals(source[i]))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Returns a random object from the list. It uses a history to prevent repeating
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tracks">The list of the tracks</param>
        /// <returns></returns>
        public static T GetRandomObject<T>(this IList<T> tracks)
        {
            if (tracks.Count == 1)
                return tracks[0];
            var history = tracks.GetShuffleHistory();

            foreach (var track in history.Where(track => !tracks.Any(x => x.Equals(track))))
            {
                history.Remove(track); //We remove all tracks from the history which aren't in the playlist any more
            }

            var shuffleList = tracks.Where(x => !history.Contains(x)).ToList(); //We search all tracks which are available and not in the history
            if (shuffleList.Count == 1) //If there is only one item, we return that
                return shuffleList[0];

            if (shuffleList.Count == 0)
            {
                history.Clear();
                shuffleList.AddRange(tracks);
            }

            return shuffleList[PrivateRandom.Next(shuffleList.Count)];
        }

        /// <summary>
        /// Returns the previous object, relative to <see cref="currentObject"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tracks"></param>
        /// <param name="currentObject"></param>
        /// <returns></returns>
        public static T GetPreviousObject<T>(this IList<T> tracks, object currentObject)
        {
            if (tracks.Count == 1 || currentObject == null || !tracks.Any(x => x.Equals(currentObject)))
                return tracks[0];

            var newIndex = tracks.IndexOf(currentObject) - 1;
            if (newIndex < 0)
                newIndex = tracks.Count - 1;
            return tracks[newIndex];
        }

        /// <summary>
        /// Returns the next object, relative to <see cref="currentObject"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tracks"></param>
        /// <param name="currentObject"></param>
        /// <returns></returns>
        public static T GetNextObject<T>(this IList<T> tracks, object currentObject)
        {
            if (tracks.Count == 1 || currentObject == null || !tracks.Any(x => x.Equals(currentObject)))
                return tracks[0];

            var newIndex = tracks.IndexOf(currentObject) + 1;
            if (newIndex >= tracks.Count)
                newIndex = 0;

            return tracks[newIndex];
        }
    }
}