using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hurricane.Model.AudioEngine;
using Hurricane.Model.Music;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.Model.Notifications;
using File = TagLib.File;

namespace Hurricane.Model.Data
{
    public class TrackImporter : IProgressReporter
    {
        private readonly MusicDataManager _musicDataManager;
        private bool _cancel;
        private bool _isEnabled;

        public TrackImporter(MusicDataManager musicDataManager)
        {
            _musicDataManager = musicDataManager;
        }

        public event EventHandler<double> ProgressChanged;
        public event EventHandler<string> ProgressMessageChanged;
        public event EventHandler Finished;

        public async Task ImportTracks(IEnumerable<FileInfo> files, UserPlaylist playlist)
        {
            if(_isEnabled)
                throw new InvalidOperationException("The TrackImporter is already running");
            _isEnabled = true;

            var extensions = _musicDataManager.MusicManager.AudioEngine.SupportedExtensions;

            string extension;
            AudioInformation audioInformation = null;

            var filesToImport = files.Where(fileInfo => fileInfo.Exists && !string.IsNullOrEmpty(fileInfo.Extension)).ToList();
            var allFilesCount = (double) filesToImport.Count;
            var artistRegex = new Regex("(?<artist>([a-zA-Z].+?)) -");

            for (int i = 0; i < filesToImport.Count; i++)
            {
                var fileInfo = filesToImport[i];
                ProgressMessageChanged?.Invoke(this, $"\"{fileInfo.Name}\"");
                ProgressChanged?.Invoke(this, i / allFilesCount);

                extension = fileInfo.Extension.Remove(0, 1);
                if (!extensions.Any(x => string.Equals(x, extension))) continue; //If the audio engine doesn't know the extension, skip
                if (!(await Task.Run(() => _musicDataManager.MusicManager.AudioEngine.TestAudioFile(fileInfo.FullName, out audioInformation)))) //If the audio engine can't open the track, skip
                    continue;

                var track = new LocalPlayable
                {
                    Extension = extension.ToUpper(),
                    TrackPath = fileInfo.FullName,
                    Duration = audioInformation.Duration,
                    SampleRate = Math.Round(audioInformation.SampleRate/1000d, 1)
                };
                string artistName;
                Artist artist = null;

                try
                {
                    using (var tagLibInfo = File.Create(fileInfo.FullName)) //We look into the tags. Perhaps we'll find something interesting
                    {
                        track.Title = tagLibInfo.Tag.Title;

                        if (!string.IsNullOrEmpty(tagLibInfo.Tag.MusicBrainzArtistId))
                            artist = await GetArtistByMusicBrainzId(tagLibInfo.Tag.MusicBrainzArtistId); //Ui, that's awesome
                        track.Album = tagLibInfo.Tag.Album;
                        artistName = tagLibInfo.Tag.FirstPerformer ?? tagLibInfo.Tag.FirstAlbumArtist; //Both is okay
                    }
                }
                catch (Exception)
                {
                    //Do nothing
                    artistName = null;
                }

                if (string.IsNullOrEmpty(track.Title)) //If the title wasn't found in the tags or if we couldn't open the tags
                    track.Title = Path.GetFileNameWithoutExtension(fileInfo.FullName);

                if (artist == null) //Perhaps we found the artist thought the music brainz id
                {
                    if (string.IsNullOrEmpty(artistName)) //If we haven't the artist name
                    {
                        var match = artistRegex.Match(Path.GetFileNameWithoutExtension(fileInfo.FullName));
                        if (match.Success)
                            artistName = match.Groups["artist"].Value;
                    }

                    if (!string.IsNullOrEmpty(artistName)) //Perhaps regex didn't match
                    {
                        artist = await _musicDataManager.LastfmApi.SearchArtist(artistName);
                    }
                }

                if (artist == null)
                    artist = _musicDataManager.ArtistManager.UnkownArtist;

                if (!_musicDataManager.ArtistManager.ArtistDictionary.ContainsKey(artist.Guid))
                    _musicDataManager.ArtistManager.ArtistDictionary.Add(artist.Guid, artist);

                track.Artist = artist;

                var trackId = Guid.NewGuid();
                _musicDataManager.Tracks.Add(trackId, track);
                playlist?.AddTrack(track);

                if (_cancel)
                    break;
            }
            Finished?.Invoke(this, EventArgs.Empty);
            _isEnabled = false;
            _cancel = false;
        }

        public Task ImportTracks(IEnumerable<FileInfo> files)
        {
            return ImportTracks(files, null);
        }

        public Task ImportDirectory(DirectoryInfo directoryInfo, bool goDeep, UserPlaylist playlist)
        {
            return ImportTracks(directoryInfo.GetFiles("*.*",
                goDeep ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly), playlist);
        }

        public Task ImportDirectory(DirectoryInfo directoryInfo, bool goDeep)
        {
            return ImportTracks(directoryInfo.GetFiles("*.*",
                goDeep ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly), null);
        }

        public Task ImportDirectory(DirectoryInfo directoryInfo)
        {
            return ImportDirectory(directoryInfo, true);
        }

        private async Task<Artist> GetArtistByMusicBrainzId(string musicBrainzId)
        {
            var temp =
                _musicDataManager.ArtistManager.ArtistDictionary.Where(
                    x =>
                        string.Equals(x.Value.MusicBrainzId, musicBrainzId,
                            StringComparison.OrdinalIgnoreCase)).ToList();

            if (temp.Count > 0)
                return temp[0].Value;

            return await _musicDataManager.LastfmApi.GetArtistByMusicbrainzId(musicBrainzId, CultureInfo.CurrentCulture);
        }

        public void Cancel()
        {
            _cancel = true;
        }
    }
}