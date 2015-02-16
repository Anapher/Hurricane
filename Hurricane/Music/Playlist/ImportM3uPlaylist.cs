using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music.Playlist
{
    [PlaylistFormat]
    static class ImportM3uPlaylist
    {
        public static PlaylistFormat GetFormat()
        {
            return new PlaylistFormat("WinAmp Playlist", new string[] { ".m3u", ".pls" }, IsSupported, Import);
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

            if (Track.LocalTrack.IsSupported(new FileInfo(line)))
                return true;

            return false;
        }

        public static IPlaylist Import(string base_path, StreamReader reader)
        {
            var playlist = new NormalPlaylist();
            Track.LocalTrack track = null;

            for (int track_number = 1; ; )
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
                        var parts = split.Split(new char[] { ',' }, 2);

                        if (parts.Length == 2)
                        {
                            track = new Track.LocalTrack { Title = parts[1].Trim() };
                            track.ResetDuration(new TimeSpan(0, 0, int.Parse(parts[0])));
                        }
                        else
                        {
                            track = new Track.LocalTrack { Title = split.Trim() };
                        }
                    }
                    catch (Exception ex)
                    {
                        // ignore it
                        System.Diagnostics.Debug.WriteLine("M3U ext track import error: " + ex.Message);
                    }
                }
                else if (line[0] != '#')	// skip comments
                {
                    if (track == null)
                        track = new Track.LocalTrack();

                    track.Path = Path.Combine(base_path, line);
                    track.TrackNumber = track_number++;

                    playlist.AddTrack(track);

                    track = null;
                }
            }

            return playlist;
        }
    }
}
