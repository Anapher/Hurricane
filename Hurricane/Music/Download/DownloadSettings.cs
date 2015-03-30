using System;
using System.IO;
using System.Xml.Serialization;
using Hurricane.Music.Track;

namespace Hurricane.Music.Download
{
    public class DownloadSettings
    {
        public bool AddTags { get; set; }

        private string _downloadFolder;
        [XmlIgnore]
        public string DownloadFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_downloadFolder) || !Directory.Exists(_downloadFolder))
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

                return _downloadFolder;
            }
            set { _downloadFolder = value; }
        }

        [XmlElement(ElementName = "DownloadFolder")]
        public string DownloadFolder2Serialize
        {
            get { return _downloadFolder; }
            set { _downloadFolder = value; }
        }


        public bool IsConverterEnabled { get; set; }
        public AudioBitrate Bitrate { get; set; }
        public AudioFormat Format { get; set; }

        public string GetExtension(IDownloadable track)
        {
            return GetExtension(DownloadManager.GetExtension(track));
        }

        public string GetExtension(string defaultExtension)
        {
            switch (Format)
            {
                case AudioFormat.Copy:
                    return defaultExtension;
                case AudioFormat.MP3:
                    return ".mp3";
                case AudioFormat.AAC:
                    return ".aac";
                case AudioFormat.WMA:
                    return ".wma";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetDefault()
        {
            IsConverterEnabled = false;
            Bitrate = AudioBitrate.B256;
            Format = AudioFormat.Copy;
            AddTags = true;
            DownloadFolder2Serialize = string.Empty;
        }

        public DownloadSettings Clone()
        {
            return new DownloadSettings
            {
                IsConverterEnabled = IsConverterEnabled,
                AddTags = AddTags,
                Bitrate = Bitrate,
                DownloadFolder2Serialize = DownloadFolder2Serialize,
                Format = Format
            };
        }
    }
}