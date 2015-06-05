using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hurricane.Model.AudioEngine;
using Hurricane.Model.DataApi;
using Hurricane.Model.Music;
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
            if(_isEnabled)
                throw new InvalidOperationException("The TrackImporter is already running");
            _isEnabled = true;

            var extensions = _musicDataManager.MusicManager.AudioEngine.SupportedExtensions;

            string extension;
            AudioInformation audioInformation = null;

            var filesToImport = files.Where(fileInfo => fileInfo.Exists && !string.IsNullOrEmpty(fileInfo.Extension)).ToList();
            var allFilesCount = (double) filesToImport.Count;
            var filenameRegex = new Regex("(?<artist>([a-zA-Z].+?)) - (?<title>(.*?))[[(]");

            for (int i = 0; i < filesToImport.Count; i++)
            {
                var fileInfo = filesToImport[i];
                ProgressMessageChanged?.Invoke(this, $"\"{fileInfo.Name}\"");
                ProgressChanged?.Invoke(this, i / allFilesCount);

                extension = fileInfo.Extension.Remove(0, 1);
                if (!extensions.Any(x => string.Equals(x, extension, StringComparison.OrdinalIgnoreCase))) continue; //If the audio engine doesn't know the extension, skip
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
                string title;
                Artist artist = null;
                ImageProvider cover = null;

                try
                {
                    using (var tagLibInfo = File.Create(fileInfo.FullName)) //We look into the tags. Perhaps we'll find something interesting
                    {
                        title = tagLibInfo.Tag.Title;
                        if (!string.IsNullOrEmpty(tagLibInfo.Tag.MusicBrainzArtistId))
                            artist = await GetArtistByMusicBrainzId(tagLibInfo.Tag.MusicBrainzArtistId); //Ui, that's awesome
                        artistName = tagLibInfo.Tag.FirstPerformer ?? tagLibInfo.Tag.FirstAlbumArtist; //Both is okay
                        if (tagLibInfo.Tag.Pictures.Any())
                        {
                            //Debug.Assert(tagLibInfo.Tag.Pictures.Length > 1, "tagLibInfo.Tag.Pictures.Length > 1");
                            cover = new TagImage(fileInfo.FullName);
                        }
                        track.Bitrate = tagLibInfo.Properties.AudioBitrate;
                        if (!string.IsNullOrEmpty(tagLibInfo.Tag.Album))
                        {
                            track.Album =
                                _musicDataManager.Albums.AlbumDicitionary.FirstOrDefault(
                                    x =>
                                        string.Equals(x.Value.Name, tagLibInfo.Tag.Album,
                                            StringComparison.OrdinalIgnoreCase)).Value;

                            if (track.Album == null)
                            {
                                var album = new Album {Name = tagLibInfo.Tag.Album, Guid = Guid.NewGuid()};
                                track.Album = album;
                                _musicDataManager.Albums.AlbumDicitionary.Add(album.Guid, album);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //Do nothing
                    artistName = null;
                    title = null;
                }

                if (title == null || (artistName == null && artist == null))
                {
                    var match = filenameRegex.Match(Path.GetFileNameWithoutExtension(fileInfo.FullName));
                    if (match.Success)
                    {
                        if (title == null)
                            title = match.Groups["title"].Value;
                        if (artistName == null)
                            artistName = match.Groups["artist"].Value;
                    }
                    else
                    {
                        title = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                        artistName = string.Empty;
                    }
                }

                var trackInfo = await _musicDataManager.LastfmApi.GetTrackInformation(title, artistName);
                if (trackInfo != null)
                {
                    track.Title = trackInfo.Name;
                    track.MusicBrainzId = trackInfo.MusicBrainzId;
                    track.Cover = cover ?? trackInfo.CoverImage;
                    if (!string.IsNullOrEmpty(trackInfo.MusicBrainzId))
                    {
                        var artistId = await MusicBrainzApi.GetArtistIdByTrackId(trackInfo.MusicBrainzId);
                        if (!string.IsNullOrEmpty(artistId))
                        {
                            artist =
                                await
                                    _musicDataManager.LastfmApi.GetArtistByMusicbrainzId(artistId,
                                        CultureInfo.CurrentCulture);
                        }
                    }
                    artistName = trackInfo.Artist;
                }
                else
                {
                    track.Title = title;
                }

                if (artist == null)
                {
                    track.Artist =
                        _musicDataManager.Artists.ArtistDictionary.FirstOrDefault(
                            x => string.Equals(x.Value.Name, artistName, StringComparison.OrdinalIgnoreCase)).Value ??
                        await _musicDataManager.LastfmApi.SearchArtist(artistName);
                }
                else
                {
                    track.Artist = _musicDataManager.Artists.ArtistDictionary.FirstOrDefault(
                        x => string.Equals(x.Value.Name, artist.Name, StringComparison.OrdinalIgnoreCase)).Value ??
                                   artist;
                }

                if (track.Artist == null)
                    track.Artist = _musicDataManager.Artists.UnkownArtist;

                if (!_musicDataManager.Artists.ArtistDictionary.ContainsKey(track.Artist.Guid))
                    _musicDataManager.Artists.ArtistDictionary.Add(track.Artist.Guid, track.Artist);

                _musicDataManager.Tracks.AddTrack(track);
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
                _musicDataManager.Artists.ArtistDictionary.Where(
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