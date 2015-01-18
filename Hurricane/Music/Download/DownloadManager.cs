using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using Hurricane.Settings;
using Hurricane.ViewModelBase;

namespace Hurricane.Music.Download
{
    [Serializable]
    public class DownloadManager : PropertyChangedBase, IDisposable
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

        public void AddEntry(string url, string trackname)
        {
            HasEntries = true;
            var downloadDirectory = new DirectoryInfo(DownloadDirectory);
            if (!downloadDirectory.Exists) downloadDirectory.Create();
            var entry = new DownloadEntry
            {
                IsWaiting = true,
                DownloadUrl = url,
                Filename = Path.Combine(downloadDirectory.FullName, Utilities.GeneralHelper.EscapeFilename(trackname)),
                Trackname = trackname
            };
            Entries.Add(entry);
            _hasToCheck = true;
            DownloadTracks();
        }

        private bool _isRunning;
        private bool _hasToCheck;
        private DownloadEntry _currentEntry;

        private async void DownloadTracks()
        {
            while (true)
            {
                if (_isRunning) return;
                _isRunning = true;
                _hasToCheck = false;

                foreach (var entry in Entries.Where(x => !x.IsDownloaded).ToList())
                {
                    if (_client == null)
                    {
                        _client = new WebClient { Proxy = null };
                        _client.DownloadProgressChanged += _client_DownloadProgressChanged;
                    }
                    entry.IsWaiting = false;
                    _currentEntry = entry;
                    await _client.DownloadFileTaskAsync(entry.DownloadUrl, entry.Filename);
                    entry.IsDownloaded = true;
                }

                _isRunning = false;
                if (_hasToCheck) continue;
                break;
            }
        }

        void _client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (_currentEntry != null) _currentEntry.Progress = e.ProgressPercentage;
        }

        private WebClient _client;
        public DownloadManager()
        {
            Entries = new ObservableCollection<DownloadEntry>();
            DownloadDirectory = "Downloads";
            AddTagsToDownloads = true;
        }

        public void Dispose()
        {
            if (_client != null) _client.Dispose();
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
