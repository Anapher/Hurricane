using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Hurricane.Music.Track;
using Hurricane.Utilities;
using Hurricane.ViewModelBase;
using TagLib;
using File = TagLib.File;

namespace Hurricane.Music.Download
{
    [Serializable]
    public class DownloadManager : PropertyChangedBase
    {
        [XmlIgnore]
        public ObservableCollection<DownloadEntry> Entries { get; set; }

        private bool _isOpen;
        [XmlIgnore]
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                SetProperty(value, ref _isOpen);
            }
        }

        public void AddEntry<T>(T download, DownloadSettings settings, string fileName) where T : IDownloadable, IMusicInformation
        {
            HasEntries = true;
            var entry = new DownloadEntry
            {
                IsWaiting = true,
                DownloadFilename = fileName,
                Trackname = download.DownloadFilename,
                DownloadParameter = download.DownloadParameter,
                DownloadMethod = download.DownloadMethod,
                MusicInformation = download,
                DownloadSettings = settings.Clone()
            };

            Entries.Add(entry);
            _hasToCheck = true;
            DownloadTracks();
        }

        private bool _isRunning;
        private bool _hasToCheck;

        private async void DownloadTracks()
        {
            while (true)
            {
                if (_isRunning) return;
                _isRunning = true;
                _hasToCheck = false;

                foreach (var entry in Entries.Where(x => !x.IsDownloaded).ToList())
                {
                    entry.IsWaiting = false;
                    var currentEntry = entry;
                    await DownloadAndConfigureTrack(entry, entry.MusicInformation, entry.DownloadFilename, d => currentEntry.Progress = d, entry.DownloadSettings);
                    entry.IsDownloaded = true;
                }

                _isRunning = false;
                if (_hasToCheck) continue;
                break;
            }
        }

        private async static Task<bool> DownloadTrack(IDownloadable download, string fileName, Action<double> progressChangedAction)
        {
            try
            {
                switch (download.DownloadMethod)
                {
                    case DownloadMethod.SoundCloud:
                        await SoundCloudDownloader.DownloadSoundCloudTrack(download.DownloadParameter, fileName, progressChangedAction);
                        break;
                    case DownloadMethod.youtube_dl:
                        await youtube_dl.Instance.DownloadYouTubeVideo(download.DownloadParameter, fileName, progressChangedAction);
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string GetExtension(IDownloadable track)
        {
            switch (track.DownloadMethod)
            {
                case DownloadMethod.SoundCloud:
                    return ".mp3";
                case DownloadMethod.youtube_dl:
                    return ".m4a";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async static Task<bool> DownloadAndConfigureTrack(IDownloadable downloadInformation, IMusicInformation musicInformation, string fileName, Action<double> progressChangedAction, DownloadSettings settings)
        {
            if (!await DownloadTrack(downloadInformation, fileName, progressChangedAction))
            {
                return false;
            }

            if (settings.IsConverterEnabled)
            {
                var oldFile = new FileInfo(fileName);
                oldFile.MoveTo(GeneralHelper.GetFreeFileName(oldFile.Directory, oldFile.Extension).FullName); //We move the downloaded file to a temp location
                await ffmpeg.ConvertFile(oldFile.FullName, fileName, settings.Bitrate, settings.Format);

            }

            //TagLib# destroys all aac files...
            if (settings.AddTags && settings.Format != AudioFormat.AAC) await AddTags(musicInformation, fileName);
            return true;
        }

        public async static Task AddTags(IMusicInformation information, string path)
        {
            var filePath = new FileInfo(path);
            if (!filePath.Exists) return;
            try
            {
                using (var file = File.Create(filePath.FullName))
                {
                    file.Tag.Album = information.Album;
                    file.Tag.Performers = new[] { information.Artist };
                    file.Tag.Year = information.Year;
                    if (information.Genres != null)
                        file.Tag.Genres = information.Genres.Split(new[] { ", " }, StringSplitOptions.None);
                    file.Tag.Title = information.Title;
                    var image = await information.GetImage();
                    if (image != null)
                    {
                        byte[] data;
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(image));
                        using (MemoryStream ms = new MemoryStream())
                        {
                            encoder.Save(ms);
                            data = ms.ToArray();
                        }
                        file.Tag.Pictures = new IPicture[] { new Picture(new ByteVector(data, data.Length)) };
                    }
                    await Task.Run(() => file.Save());
                }
            }
            catch (CorruptFileException)
            {
            }
        }

        public DownloadManager()
        {
            Entries = new ObservableCollection<DownloadEntry>();
            SelectedService = 0;
            Searches = new ObservableCollection<string>();
        }

        #region Settings

        private int _selectedService;
        public int SelectedService
        {
            get { return _selectedService; }
            set
            {
                SetProperty(value, ref _selectedService);
            }
        }

        public ObservableCollection<string> Searches { get; set; }


        private bool _hasEntries;
        [XmlIgnore]
        public bool HasEntries
        {
            get { return _hasEntries; }
            set
            {
                SetProperty(value, ref _hasEntries);
            }
        }

        #endregion
    }
}
