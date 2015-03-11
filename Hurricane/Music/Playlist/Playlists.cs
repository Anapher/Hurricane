using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hurricane.Music.CustomEventArgs;
using Hurricane.Music.Track;

namespace Hurricane.Music.Playlist
{
    // Collection of play list importers (classes with [PlaylistFormat] attribute)
    // Use Playlists.Import to read supported play list file or
    // use Playlists.ImportFiles to read arbitrary files

    public static class Playlists
    {
        // imports play list and returns IPlaylist or null if list cannot be read
        public async static Task<IPlaylist> Import(string filePath)
        {
            return await Task.Run(() => IsSupported(filePath) ? DoImport(filePath) : null);
        }

        public static bool IsSupported(string filePath)
        {
            CheckRegistration();
            return MatchExtension(filePath) != null;
        }

        public static string[] GetSupportedFileExtensions()
        {
            CheckRegistration();
            var formats = new List<string>();
            foreach (var format in Formats)
            {
                formats.AddRange(format.SupportedExtensions);
            }
            return formats.ToArray();
        }

        public static IPlaylist DoImport(string filePath)
        {
            CheckRegistration();

            var format = MatchExtension(filePath);
            if (format == null)
                return null;

            using (var reader = new StreamReader(filePath))
                if (!format.IsPlaylistSupported(reader))
                    return null;

            var basePath = Path.GetDirectoryName(filePath);

            using (var reader = new StreamReader(filePath))
                return format.ImportTracks(basePath, reader);
        }

        // import all files:
        //   if some of them are playlists, read them and return tracks;
        //   turn normal sources (mp3, wav, etc.) into tracks
        public static IEnumerable<PlayableBase> ImportFiles(IEnumerable<string> paths, EventHandler<TrackImportProgressChangedEventArgs> progress)
        {
            int index = 0;
            var count = paths.Count();

            foreach (var fi in paths.Select(x => new FileInfo(x)))
            {
                if (fi.Exists)
                {
                    if (progress != null)
                        progress(null, new TrackImportProgressChangedEventArgs(index, count, fi.Name));

                    if (IsSupported(fi.FullName))  // playlist?
                    {
                        var playlist = DoImport(fi.FullName);

                        if (playlist != null)
                            foreach (var track in playlist.Tracks)
                                yield return track;
                    }
                    else
                    {
                        yield return new LocalTrack { Path = fi.FullName };
                    }
                }
                ++index;
            }
        }

        static PlaylistFormat MatchExtension(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentException();
            var ext = Path.GetExtension(filePath).ToLower();
            return Formats.Find(c => c.SupportedExtensions.Contains(ext));
        }

        static readonly List<PlaylistFormat> Formats = new List<PlaylistFormat>();

        static void CheckRegistration()
        {
            if (Formats.Count == 0)
                RunRegistration();
        }

        static void RunRegistration()
        {
            var formats = typeof(Playlists).Assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(PlaylistFormatAttribute), false).Length > 0);

            foreach (var format in formats)
            {
                var get = format.GetMethod("GetFormat", BindingFlags.Public | BindingFlags.Static);
                var info = get.Invoke(null, null) as PlaylistFormat;
                Debug.Assert(info != null, "Missing PlaylistFormat info for " + format.Name);
                Register(info);
            }
        }

        static void Register(PlaylistFormat format)
        {
#if DEBUG
            // check for duplicates
            foreach (var ext in format.SupportedExtensions)
                Debug.Assert(Formats.Find(f => f.SupportedExtensions.Contains(ext)) == null, "File extensions are already registered. New format: " + format.Name);
#endif
            Formats.Add(format);
        }

    }

    public class PlaylistFormat
    {
        public PlaylistFormat(string name, string[] extensions, Func<StreamReader, bool> isSupported, Func<string, StreamReader, IPlaylist> importTracks)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(name));
            Debug.Assert(extensions != null);
            Debug.Assert(isSupported != null);
            Debug.Assert(importTracks != null);

            Name = name;
            SupportedExtensions = extensions;
            IsPlaylistSupported = isSupported;
            ImportTracks = importTracks;
        }

        // name of the playlist format
        public string Name { get; set; }
        // file extension(s)
        public string[] SupportedExtensions { get; set; }
        // read file content to see if it is supported
        public Func<StreamReader, bool> IsPlaylistSupported { get; set; }
        // import playlist
        public Func<string, StreamReader, IPlaylist> ImportTracks { get; set; }
    }

    public class PlaylistFormatAttribute : Attribute
    { }
}
