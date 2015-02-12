using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurricane.Music.MusicDatabase.EventArgs;

namespace Hurricane.Music.Playlist
{
    // Collection of play list importers (classes with [PlaylistFormat] attribute)
    // Use Playlists.Import to read supported play list file or
    // use Playlists.ImportFiles to read arbitrary files

    public static class Playlists
    {
        // imports play list and returns IPlaylist or null if list cannot be read
        public async static Task<IPlaylist> Import(string file_path)
        {
            return await Task.Run(delegate
            {
                return Playlists.IsSupported(file_path) ? Playlists.DoImport(file_path) : null;
            });
        }

        public static bool IsSupported(string file_path)
        {
            CheckRegistration();
            return MatchExtension(file_path) != null;
        }

        public static IPlaylist DoImport(string file_path)
        {
            CheckRegistration();

            var format = MatchExtension(file_path);
            if (format == null)
                return null;

            using (var reader = new StreamReader(file_path))
                if (!format.IsPlaylistSupported(reader))
                    return null;

            var base_path = Path.GetDirectoryName(file_path);

            using (var reader = new StreamReader(file_path))
                return format.ImportTracks(base_path, reader);
        }

        // import all files:
        //   if some of them are playlists, read them and return tracks;
        //   turn normal sources (mp3, wav, etc.) into tracks
        public static IEnumerable<Track.PlayableBase> ImportFiles(IEnumerable<string> paths, EventHandler<TrackImportProgressChangedEventArgs> progress)
        {
            int index = 0;
            var count = paths.Count();

            foreach (var path in paths)
            {
                FileInfo fi = new FileInfo(path);
                if (fi.Exists)
                {
                    if (progress != null)
                        progress(null, new TrackImportProgressChangedEventArgs(index, count, fi.Name));

                    if (IsSupported(path))  // playlist?
                    {
                        var playlist = DoImport(path);

                        if (playlist != null)
                            foreach (var track in playlist.Tracks)
                                yield return track;
                    }
                    else
                    {
                        yield return new Track.LocalTrack { Path = fi.FullName };
                    }
                }
                ++index;
            }
        }

        static PlaylistFormat MatchExtension(string file_path)
        {
            var ext = Path.GetExtension(file_path).ToLower();
            return formats_.Find(c => c.SupportedExtensions.Contains(ext));
        }

        static List<PlaylistFormat> formats_ = new List<PlaylistFormat>();

        static void CheckRegistration()
        {
            if (formats_.Count == 0)
                RunRegistration();
        }

        static void RunRegistration()
        {
            var formats = typeof(Playlists).Assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(PlaylistFormatAttribute), false).Length > 0);

            foreach (var format in formats)
            {
                var get = format.GetMethod("GetFormat", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var info = get.Invoke(null, null) as PlaylistFormat;
                System.Diagnostics.Debug.Assert(info != null, "Missing PlaylistFormat info for " + format.Name);
                if (info != null)
                    Register(info);
            }
        }

        static void Register(PlaylistFormat format)
        {
#if DEBUG
            // check for duplicates
            foreach (var ext in format.SupportedExtensions)
                System.Diagnostics.Debug.Assert(formats_.Find(f => f.SupportedExtensions.Contains(ext)) == null, "File extensions are already registered. New format: " + format.Name);
#endif
            formats_.Add(format);
        }

    }

    public class PlaylistFormat
    {
        public PlaylistFormat(string name, string[] extensions, Func<StreamReader, bool> is_supported, Func<string, StreamReader, IPlaylist> import_tracks)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(name));
            System.Diagnostics.Debug.Assert(extensions != null);
            System.Diagnostics.Debug.Assert(is_supported != null);
            System.Diagnostics.Debug.Assert(import_tracks != null);

            Name = name;
            SupportedExtensions = extensions;
            IsPlaylistSupported = is_supported;
            ImportTracks = import_tracks;
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
