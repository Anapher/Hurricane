using System;
using System.Diagnostics;
using System.IO;
using Hurricane.Music.Track;
using Hurricane.Utilities;

namespace Hurricane.Music.Playlist
{
    [PlaylistFormat]
    public static class ImportM3UPlaylist
    {
        public static PlaylistFormat GetFormat()
        {
            return new PlaylistFormat("WinAmp Playlist", new[] { ".m3u", ".pls" }, IsSupported, Import);
        }

        public static bool IsSupported(StreamReader reader)
        {
            var line = reader.ReadLine();
            if (line == null)
                return false;

            line = line.Trim();

            if (line == "#EXTM3U")
                return true;

            if (line.StartsWith("http"))
                return true;

            if (LocalTrack.IsSupported(new FileInfo(line)))
                return true;

            return false;
        }

        public static IPlaylist Import(string basePath, StreamReader reader)
        {
            var playlist = new NormalPlaylist();

            for (int trackNumber = 1; ; )
            {
                var line = reader.ReadLine();

                if (line == null)
                    break;

                line = line.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.ToUpper().StartsWith("#EXTINF:"))
                {
                    try
                    {
                        var split = line.Substring(8).TrimStart(',');
                        var parts = split.Split(new[] { ',' }, 2);

                        var location = reader.ReadLine();
                        if (string.IsNullOrEmpty(location))
                            break; //corrupt file

                        PlayableBase track;
                        if (location.StartsWith("http:"))
                        {
                            track = new CustomStream {StreamUrl = location};
                        }
                        else
                        {
                        
                         var localTrack = new LocalTrack();
                            var file = new FileInfo(FileSystemHelper.GetAbsolutePath(location, basePath));
                            if (!file.Exists)
                                continue;
                            localTrack.Path = file.FullName;
                            track = localTrack;
                        }

                        track.Title = parts.Length == 2
                            ? parts[1].Trim()
                            : split.Trim();
                        track.TrackNumber = trackNumber++;

                        playlist.AddTrack(track);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        // ignore it
                        Debug.WriteLine("M3U ext track import error: " + ex.Message);
                    }
                }

                if (line.StartsWith("http"))
                {
                    Uri uri;
                    try
                    {
                        uri = new Uri(line);
                    }
                    catch (Exception)
                    {
                        Debug.Print("M3U track import error: invalid uri - '{0}'", line);
                        continue;
                    }
                    playlist.AddTrack(new CustomStream { StreamUrl = line, Title = uri.Host, TrackNumber = trackNumber++ });
                    continue;
                }

                if (line[0] != '#')	// skip comments
                {
                    var file = new FileInfo(FileSystemHelper.GetAbsolutePath(line, basePath));
                    if (!file.Exists)
                        continue;

                    playlist.AddTrack(new LocalTrack
                    {
                        Path = file.FullName,
                        TrackNumber = trackNumber++,
                        Title = Path.GetFileNameWithoutExtension(file.FullName)
                    });
                }
            }

            return playlist;
        }
    }
}