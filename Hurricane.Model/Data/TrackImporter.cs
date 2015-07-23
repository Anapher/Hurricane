using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hurricane.Model.AudioEngine;
using Hurricane.Model.DataApi;
using Hurricane.Model.Music.Imagment;
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
            if (_isEnabled)
                throw new InvalidOperationException("The TrackImporter is already running");
            _isEnabled = true;

            var extensions = _musicDataManager.MusicManager.AudioEngine.SupportedExtensions;
            var filesToImport = files.Where(fileInfo => fileInfo.Exists && !string.IsNullOrEmpty(fileInfo.Extension)).ToList();
            var allFilesCount = (double)filesToImport.Count;

            for (int i = 0; i < filesToImport.Count; i++)
            {
                var fileInfo = filesToImport[i];
                ProgressMessageChanged?.Invoke(this, $"\"{fileInfo.Name}\"");
                ProgressChanged?.Invoke(this, i / allFilesCount);

                var track = await GetTrack(fileInfo, extensions);

                if (playlist != null && !playlist.Tracks.Contains(track))
                    playlist.AddTrack(track);

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

        public void Cancel()
        {
            _cancel = true;
        }

        private async Task<PlayableBase> GetTrack(FileInfo fileInfo, List<string> supportedExtensions)
        {
            var extension = fileInfo.Extension.Remove(0, 1);

            //--- STEP 1: Check the track ---
            //Check the extension
            if (!supportedExtensions.Any(x => string.Equals(x, extension, StringComparison.OrdinalIgnoreCase)))
                return null;

            AudioInformation audioInformation = null;
            //Check the file
            if (
                !(await
                    Task.Run(
                        () =>
                            _musicDataManager.MusicManager.AudioEngine.TestAudioFile(fileInfo.FullName,
                                out audioInformation)))) //If the audio engine can't open the track, skip
                return null;

            LocalPlayable track;
            //--- STEP 2: Get information from the file ---
            //Search if track is already in the database
            if (SearchTrack(fileInfo.FullName, out track))
                return track;

            //Create a new track with some information we already have
            track = new LocalPlayable
            {
                Extension = extension.ToUpper(),
                TrackPath = fileInfo.FullName,
                Duration = audioInformation.Duration,
                SampleRate = Math.Round(audioInformation.SampleRate / 1000d, 1)
            };

            string filenameArtistName = null;
            string tagArtistName = null;
            string internetArtistName = null;

            string albumName = null;
            string title = null;

            /*
                Information priority:
                1. Tag
                2. Internet
                3. Filename
            */

            try
            {
                //Let's have a look in the tags
                using (var tagLibInfo = File.Create(fileInfo.FullName)) //We look into the tags. Perhaps we'll find something interesting
                {
                    track.Title = tagLibInfo.Tag.Title;
                    if (!string.IsNullOrEmpty(tagLibInfo.Tag.MusicBrainzArtistId))
                        track.Artist = await GetArtistByMusicBrainzId(tagLibInfo.Tag.MusicBrainzArtistId); //Ui, that's awesome
                    else
                        tagArtistName = tagLibInfo.Tag.FirstPerformer ?? tagLibInfo.Tag.FirstAlbumArtist; //Both is okay

                    if (tagLibInfo.Tag.Pictures.Any())
                    {
                        if (tagLibInfo.Tag.Pictures.Count() > 1)
                            Debug.Print("tagLibInfo.Tag.Pictures.Length > 1");

                        track.Cover = new TagImage(fileInfo.FullName);
                    }

                    track.Bitrate = tagLibInfo.Properties.AudioBitrate;
                    albumName = tagLibInfo.Tag.Album;
                }
            }
            catch (Exception)
            {
                //Do nothing
            }

            //At the next step, the title must have a value
            if (track.Title == null || (tagArtistName == null && track.Artist == null))
            {
                var match = Regex.Match(Path.GetFileNameWithoutExtension(fileInfo.FullName), @"(?<artist>([a-zA-Z].+?)) - (?<title>(.[^\(\[-]+))");
                if (match.Success)
                {
                    title = match.Groups["title"].Value.Trim();
                    if (tagArtistName == null)
                        filenameArtistName = match.Groups["artist"].Value;
                }
                else
                {
                    title = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                    if (tagArtistName == null)
                        filenameArtistName = string.Empty;
                }
            }

            //Now we search the track in the internet. If we have find something, we set all information which has to be set
            var trackInfo = await _musicDataManager.LastfmApi.GetTrackInformation(track.Title ?? title, track.Artist?.Name ?? tagArtistName ?? filenameArtistName);
            if (trackInfo != null)
            {
                if (track.Title == null)
                    track.Title = trackInfo.Name;

                if (!string.IsNullOrEmpty(trackInfo.MusicBrainzId))
                {
                    var temp = SearchTrackByMusicBrainzId(trackInfo.MusicBrainzId);
                    if (temp != null)
                        return temp;

                    //Check if we already have a track with this id
                    track.MusicBrainzId = trackInfo.MusicBrainzId;
                }

                if (track.Cover == null)
                    track.Cover = trackInfo.CoverImage;

                if (track.Artist == null)
                {
                    track.Artist =
                        await SearchArtistOnline(tagArtistName, trackInfo.Artist, filenameArtistName, track.MusicBrainzId);
                    if (track.Artist == null)
                        internetArtistName = trackInfo.Artist;
                }
            }
            else if(track.Title == null)
            {
                track.Title = title;
            }

            if (track.Artist == null)
            {
                var name = tagArtistName ?? internetArtistName ?? filenameArtistName;
                track.Artist = !string.IsNullOrEmpty(name) ? new Artist {Name = name} : _musicDataManager.Artists.UnknownArtist;
            }

            if (!_musicDataManager.Artists.ArtistDictionary.ContainsKey(track.Artist.Guid))
                await _musicDataManager.Artists.AddArtist(track.Artist);

            if (!string.IsNullOrWhiteSpace(albumName))
            {
                track.Album =
                    _musicDataManager.Albums.Collection.FirstOrDefault(
                        x =>
                            string.Equals(x.Value.Name, albumName,
                                StringComparison.OrdinalIgnoreCase)).Value;

                if (track.Album == null)
                {
                    var album = new Album
                    {
                        Name = albumName,
                        Guid = Guid.NewGuid()
                    };

                    await _musicDataManager.Albums.AddAlbum(album);
                    track.Album = album;
                }

                if (track.Artist != _musicDataManager.Artists.UnknownArtist &&
                    !track.Album.Artists.Contains(track.Artist))
                {
                    track.Album.Artists.Add(track.Artist);
                    await _musicDataManager.Albums.UpdateAlbumArtists(track.Album);
                }
            }

            await _musicDataManager.Tracks.AddTrack(track);
            return track;
        }

        private async Task<Artist> GetArtistByMusicBrainzId(string musicBrainzId)
        {
            var temp =
                _musicDataManager.Artists.ArtistDictionary.Where(
                    x =>
                        string.Equals(x.Value.MusicBrainzId, musicBrainzId,
                            StringComparison.OrdinalIgnoreCase)).ToList();

            if (temp.Count > 0)
                return temp[0].Value;

            var newArtist = await _musicDataManager.LastfmApi.GetArtistByMusicbrainzId(musicBrainzId, CultureInfo.CurrentCulture);
            Artist existingArtist;

            if (_musicDataManager.LastfmApi.SearchArtist(newArtist.Name, out existingArtist))
                return existingArtist;

            return newArtist;
        }

        private bool SearchTrack(string path, out LocalPlayable playable)
        {
            foreach (var playableBase in _musicDataManager.Tracks.Tracks)
            {
                var localTrack = playableBase as LocalPlayable;
                if (localTrack != null && localTrack.TrackPath == path)
                {
                    playable = localTrack;
                    return true;
                }
            }

            playable = null;
            return false;
        }

        private LocalPlayable SearchTrackByMusicBrainzId(string musicBrainzId)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            // We don't want to use linq because performance matters and linq has a really bad performance
            foreach (var track in _musicDataManager.Tracks.Tracks)
            {
                if (string.Equals(track.MusicBrainzId, musicBrainzId, StringComparison.OrdinalIgnoreCase))
                {
                    var localPlayable = track as LocalPlayable;
                    if (localPlayable == null)
                        continue;

                    return localPlayable;
                }
            }

            return null;
        }

        private async Task<Artist> SearchArtistOnline(string tagArtistName, string internetArtistName, string filenameArtistName, string trackMusicBrainzId)
        {
            Artist tempArtist;

            //we search the artist names offline.

            if (!string.IsNullOrWhiteSpace(tagArtistName) &&
                _musicDataManager.LastfmApi.SearchArtist(tagArtistName, out tempArtist))
                return tempArtist;

            if (!string.IsNullOrWhiteSpace(internetArtistName) &&
                _musicDataManager.LastfmApi.SearchArtist(internetArtistName, out tempArtist))
                return tempArtist;

            if (!string.IsNullOrWhiteSpace(filenameArtistName) &&
                _musicDataManager.LastfmApi.SearchArtist(filenameArtistName, out tempArtist))
                return tempArtist;

            if (!string.IsNullOrEmpty(trackMusicBrainzId))
            {
                var artistId = await MusicBrainzApi.GetArtistIdByTrackId(trackMusicBrainzId);
                if (!string.IsNullOrEmpty(artistId))
                {
                    var artist =
                        await
                            _musicDataManager.LastfmApi.GetArtistByMusicbrainzId(artistId,
                                CultureInfo.CurrentCulture);
                    return artist;
                }
            }

            return null;
        }
    }
}