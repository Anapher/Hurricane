using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music.Playlist
{
    [PlaylistFormat]
    static class ImportCueSheet
    {
        public static PlaylistFormat GetFormat()
        {
            return new PlaylistFormat("Cue Sheet", new string[] { ".cue" }, IsSupported, Import);
        }

        public static bool IsSupported(StreamReader reader)
        {
            var line = reader.ReadLine();
            if (line == null)
                return false;

            line = line.Trim();
            return line.StartsWith("REM") || line.StartsWith("PERFORMER") || line.StartsWith("TITLE") || line.StartsWith("FILE") || line.StartsWith("CATALOG") || Track.LocalTrack.IsSupported(new FileInfo(line));
        }

        public static IPlaylist Import(string base_path, StreamReader reader)
        {
            var playlist = new NormalPlaylist();

            var cue = new CueSharp.CueSheet(reader);

            if (cue.Tracks.Length == 0 || cue.Tracks[0].DataFile.Filename == null)
                return null;

            var genre = string.Empty;
            var year = 0u;
            if (cue.Comments != null && cue.Comments.Length > 0)
            {
                foreach (var comment in cue.Comments)
                    if (comment.StartsWith("GENRE "))
                        genre = comment.Substring(6);
                    else if (comment.StartsWith("DATE "))
                    {
                        uint.TryParse(comment.Substring(5), out year);
                        if (year <= 1000 || year >= 3000)
                            year = 0;
                    }
            }


            var audio_file = cue.Tracks[0].DataFile.Filename;

            int i = 1;

            foreach (var track in cue.Tracks)
            {
                if (track.Indices.Length == 0)
                    continue;

                TimeSpan offset = track.Offset;

                var element = new Track.LocalTrackFragment(offset, track.Title);

                if (i < cue.Tracks.Length)
                {
                    TimeSpan next_offset = cue.Tracks[i].Offset;
                    element.ResetDuration(next_offset - offset);
                }
                else
                {
                    // there's no way of knowing what's the last track's duration without decoding audio file
                    // and checking it's duration first; deferred to track info reading code
                    //element.ResetDuration(new TimeSpan(0));
                }

                element.Path = Path.Combine(base_path, audio_file);

                element.Artist = track.Performer ?? cue.Performer;
                element.Album = track.Title ?? cue.Title;
                element.TrackNumber = track.TrackNumber;
                element.Genres = genre;
                element.Year = year;

                playlist.AddTrack(element);

                ++i;
            }

            return playlist;
        }
    }
}
