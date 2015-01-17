using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using Hurricane.Settings;
using Hurricane.ViewModelBase;

namespace Hurricane.Music.Download
{
    public class DownloadManager : PropertyChangedBase, IDisposable
    {
        public ObservableCollection<DownloadEntry> Entries { get; set; }
        
        private bool _isOpen;
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
            var downloadDirectory = new DirectoryInfo(HurricaneSettings.Instance.Config.DownloadDirectory);
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

        private readonly WebClient _client;
        public DownloadManager()
        {
            Entries = new ObservableCollection<DownloadEntry>();
            _client = new WebClient{Proxy = null};
            _client.DownloadProgressChanged += _client_DownloadProgressChanged;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
