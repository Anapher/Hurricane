using System;
using System.Collections.Generic;
using System.IO;
using CueSharp;
using Hurricane.Music.Track;

namespace Hurricane.Music.Playlist
{
    [PlaylistFormat]
    static class ImportCueSheet
    {
        public static PlaylistFormat GetFormat()
        {
            return new PlaylistFormat("Cue Sheet", new [] { ".cue" }, IsSupported, Import);
        }

        public static bool IsSupported(StreamReader reader)
        {
            var line = reader.ReadLine();
            if (line == null)
                return false;

            line = line.Trim();
            return line.StartsWith("REM") || line.StartsWith("PERFORMER") || line.StartsWith("TITLE") || line.StartsWith("FILE") || line.StartsWith("CATALOG") || LocalTrack.IsSupported(new FileInfo(line));
        }

        public static IPlaylist Import(string basePath, StreamReader reader)
        {
            var playlist = new NormalPlaylist();

            var cue = new CueSheet(reader);

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


            var audioFile = cue.Tracks[0].DataFile.Filename;

            int i = 1;

            foreach (var track in cue.Tracks)
            {
                if (track.Indices.Length == 0)
                    continue;

                var offset = track.Offset;
                var duration = TimeSpan.Zero;

                if (i < cue.Tracks.Length)
                {
                    TimeSpan nextOffset = cue.Tracks[i].Offset;
                    duration = nextOffset - offset;
                }
                else
                {
                    // there's no way of knowing what's the last track's duration without decoding audio file
                    // and checking it's duration first; deferred to track info reading code
                }

                var element = new LocalTrackFragment(offset, duration, track.Title)
                {
                    Path = Path.Combine(basePath, audioFile),
                    Artist = track.Performer ?? cue.Performer,
                    Album = track.Title ?? cue.Title,
                    TrackNumber = track.TrackNumber,
                    Genres = new List<Genre> { PlayableBase.StringToGenre(genre) },
                    Year = year
                };

                playlist.AddTrack(element);

                ++i;
            }

            return playlist;
        }
    }
}
