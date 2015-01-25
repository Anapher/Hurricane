using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Hurricane.Music.Track;
using Hurricane.Settings;
using Hurricane.ViewModelBase;
using TagLib;
using TagLib.Flac;

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

        public void AddEntry<T>(T download) where T : IDownloadable, IMusicInformation
        {
            HasEntries = true;
            var downloadDirectory = new DirectoryInfo(DownloadDirectory);
            if (!downloadDirectory.Exists) downloadDirectory.Create();
            var entry = new DownloadEntry
            {
                IsWaiting = true,
                DownloadFilename = Path.Combine(downloadDirectory.FullName, Utilities.GeneralHelper.EscapeFilename(download.DownloadFilename)),
                Trackname = download.DownloadFilename,
                DownloadParameter = download.DownloadParameter,
                DownloadMethod = download.DownloadMethod,
                MusicInformation = download
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
                    await DownloadTrack(entry, entry.DownloadFilename, (d) => currentEntry.Progress = d);
                    entry.IsDownloaded = true;
                    if (AddTagsToDownloads) AddTags(entry.MusicInformation, entry.DownloadFilename);
                }

                _isRunning = false;
                if (_hasToCheck) continue;
                break;
            }
        }

        public static Task<bool> DownloadTrack(IDownloadable download, string fileName, Action<double> progressChangedAction)
        {
            switch (download.DownloadMethod)
            {
                case DownloadMethod.SoundCloud:
                    return SoundCloudDownloader.DownloadSoundCloudTrack(download.DownloadParameter, fileName, progressChangedAction);
                case DownloadMethod.youtube_dl:
                    return youtube_dl.Instance.DownloadYouTubeVideo(download.DownloadParameter, fileName, progressChangedAction);
                default:
                    throw new ArgumentException();
            }
        }

        public async static Task AddTags(IMusicInformation information, string path)
        {
            var filePath = new FileInfo(path);
            if (!filePath.Exists) return;
            using (var file = TagLib.File.Create(filePath.FullName))
            {
                file.Tag.Album = information.Album;
                file.Tag.Performers = new[] {information.Artist};
                file.Tag.Year = information.Year;
                if (information.Genres != null)
                    file.Tag.Genres = information.Genres.Split(new[] {", "}, StringSplitOptions.None);
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
                    file.Tag.Pictures = new IPicture[] {new TagLib.Picture(new ByteVector(data, data.Length))};
                }
                await Task.Run(() => file.Save());
            }
        }

        public DownloadManager()
        {
            Entries = new ObservableCollection<DownloadEntry>();
            DownloadDirectory = Path.Combine(HurricaneSettings.Instance.BaseDirectory, "Downloads");
            AddTagsToDownloads = true;
            SelectedService = 0;
            this.Searches = new ObservableCollection<string>();
        }

        #region Settings
        
        private string _downloadDirectory;
        public string DownloadDirectory
        {
            get { return _downloadDirectory; }
            set
            {
                if (SetProperty(value, ref _downloadDirectory))
                {
                    OnPropertyChanged("FolderName");
                }
            }
        }
        
        private bool _addTagsToDownloads;
        public bool AddTagsToDownloads
        {
            get { return _addTagsToDownloads; }
            set
            {
                SetProperty(value, ref _addTagsToDownloads);
            }
        }

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

        public string FolderName
        {
            get { return new DirectoryInfo(DownloadDirectory).Name; }
        }

        
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

        #region Commands
        private RelayCommand _openDownloadFolder;
        public RelayCommand OpenDownloadFolder
        {
            get
            {
                return _openDownloadFolder ?? (_openDownloadFolder = new RelayCommand(parameter =>
                {
                    Process.Start(new DirectoryInfo(DownloadDirectory).FullName);
                }));
            }
        }

        #endregion
    }
}
