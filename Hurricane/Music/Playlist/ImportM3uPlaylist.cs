using System;
using System.Diagnostics;
using System.IO;
using Hurricane.Music.Track;

namespace Hurricane.Music.Playlist
{
    [PlaylistFormat]
    static class ImportM3uPlaylist
    {
        public static PlaylistFormat GetFormat()
        {
            return new PlaylistFormat("WinAmp Playlist", new [] { ".m3u", ".pls" }, IsSupported, Import);
        }

        public static bool IsSupported(StreamReader reader)
        {
            var line = reader.ReadLine();
            if (line == null)
                return false;

            line = line.Trim();

            if (line == "#EXTM3U")
                return true;

            //TODO
            //if (line.StartsWith("http"))
            //	return true;

            if (LocalTrack.IsSupported(new FileInfo(line)))
                return true;

            return false;
        }

        public static IPlaylist Import(string basePath, StreamReader reader)
        {
            var playlist = new NormalPlaylist();
            LocalTrack track = null;

            for (int trackNumber = 1; ; )
            {
                var line = reader.ReadLine();

                if (line == null)
                    break;

                line = line.Trim();

                if (line.Length == 0)
                    continue;

                // TODO: support http

                if (line.StartsWith("#EXTINF:"))
                {
                    try
                    {
                        var split = line.Substring(8).TrimStart(',');
                        var parts = split.Split(new [] { ',' }, 2);

                        if (parts.Length == 2)
                        {
                            track = new LocalTrack { Title = parts[1].Trim() };
                            track.ResetDuration(new TimeSpan(0, 0, int.Parse(parts[0])));
                        }
                        else
                        {
                            track = new LocalTrack { Title = split.Trim() };
                        }
                    }
                    catch (Exception ex)
                    {
                        // ignore it
                        Debug.WriteLine("M3U ext track import error: " + ex.Message);
                    }
                }
                else if (line[0] != '#')	// skip comments
                {
                    if (track == null)
                        track = new LocalTrack();

                    track.Path = Path.Combine(basePath, line);
                    track.TrackNumber = trackNumber++;

                    playlist.AddTrack(track);

                    track = null;
                }
            }

            return playlist;
        }
    }
}
