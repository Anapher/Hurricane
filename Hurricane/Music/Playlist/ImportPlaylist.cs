using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music.Playlist
{
    public static class PlaylistFactory
    {
        public static void Register(PlaylistFormat format)
        {
            //TODO: check duplicates?
            formats_.Add(format);
        }

        public async static Task<IPlaylist> Import(string file_path)
        {
            return await Task.Run(delegate
            {
                return PlaylistFactory.IsSupported(file_path) ? PlaylistFactory.DoImport(file_path) : null;
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
            var formats = typeof(PlaylistFactory).Assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(PlaylistFormatAttribute), false).Length > 0);

            foreach (var format in formats)
            {
                var get = format.GetMethod("GetFormat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                var info = get.Invoke(null, null) as PlaylistFormat;
                System.Diagnostics.Debug.Assert(info != null, "Missing PlaylistFormat info for " + format.Name);
                if (info != null)
                    Register(info);
            }
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
