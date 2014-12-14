using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore.Tags.ID3;
using System.IO;
using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.MP3;
using System.Xml.Serialization;

namespace Hurricane.Music
{
    [Serializable]
    public class Track : ViewModelBase.PropertyChangedBase
    {
        #region Properties
        public string Duration { get; set; }
        public int kHz { get; set; }
        public int kbps { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public string Artist { get; set; }
        public string Extension { get; set; }
        public DateTime TimeAdded { get; set; }
        public DateTime LastTimePlayed { get; set; }

        [XmlIgnore]
        public System.Drawing.Image Image { get; set; }

        [XmlIgnore]
        public ID3v2QuickInfo TagInfo { get; set; }

        private String queueid;
        [XmlIgnore]
        public String QueueID //I know that the id should be an int, but it wouldn't make sense because what would be the id for non queued track? We would need a converter -> less performance -> string is wurf
        {
            get { return queueid; }
            set
            {
                SetProperty(value, ref queueid);
            }
        }
        private bool isplaying;
        [XmlIgnore]
        public bool IsPlaying
        {
            get { return isplaying; }
            set
            {
                SetProperty(value, ref isplaying);
            }
        }

        public TimeSpan DurationTimespan
        {
            get
            {
                return TimeSpan.ParseExact(Duration, @"mm\:ss", null);
            }
        }

        public bool TrackExists
        {
            get
            {
                return TrackInformations.Exists;
            }
        }

        private FileInfo trackinformations;
        public FileInfo TrackInformations
        {
            get
            {
                if (trackinformations == null) trackinformations = new FileInfo(Path);
                return trackinformations;
            }
        }

        public bool LoadInformations()
        {
            trackinformations = null; //to refresh the fileinfo
            FileInfo file = TrackInformations;
            try
            {
                var info = ID3v2.FromFile(Path).QuickInfo;
                if (!string.IsNullOrWhiteSpace(info.Artist))
                {
                    this.Artist = RemoveInvalidXMLChars(info.Artist);
                }
                else
                {
                    this.Artist = RemoveInvalidXMLChars(info.LeadPerformers);
                }
                if (!string.IsNullOrWhiteSpace(info.Title))
                {
                    this.Title = RemoveInvalidXMLChars(info.Title);
                }
                else
                {
                    this.Title = System.IO.Path.GetFileNameWithoutExtension(file.FullName);
                }
                this.TagInfo = info;
            }
            catch (NullReferenceException)
            {
                this.Title = System.IO.Path.GetFileNameWithoutExtension(file.FullName);
                this.TagInfo = null;
            }
            using (FileStream sr = new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                Mp3Frame frame = Mp3Frame.FromStream(sr);
                if (frame != null) { this.kbps = frame.BitRate / 1000; }
                else
                {
                    System.Diagnostics.Debug.Print("Fehler: {0}", file.FullName);
                }
            }
            this.Extension = file.Extension.ToUpper().Replace(".", string.Empty);
            try
            {
                using (IWaveSource SoundSource = CodecFactory.Instance.GetCodec(Path))
                {
                    this.kHz = SoundSource.WaveFormat.SampleRate / 1000;
                    TimeSpan duration = SoundSource.GetLength();

                    if (duration.Hours == 0)
                    {
                        this.Duration = duration.ToString(@"mm\:ss");
                    }
                    else
                    {
                        this.Duration = duration.ToString(@"hh\:mm\:ss");
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Artist))
                return string.Format("{0} - {1}", this.Artist, this.Title);

            return Title;
        }
        #endregion

        #region Methods
        public void RefreshTrackExists()
        {
            trackinformations = null;
            OnPropertyChanged("TrackExists");
        }

        /// <summary>
        /// Make ready for playing
        /// </summary>
        public void Load()
        {
            try
            {
                var info = ID3v2.FromFile(Path).QuickInfo;
                this.Image = info.Image;
            }
            catch (Exception)
            {
                this.Image = null;
            }
        }

        /// <summary>
        /// Delete all which was needed for playing
        /// </summary>
        public void Unload()
        {
            if (this.Image != null)
            {
                this.Image.Dispose();
                this.Image = null;
            }
        }

        public string GenerateHash()
        {
            using (var md5Hasher = System.Security.Cryptography.MD5.Create())
            {
                byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(this.Path));
                return BitConverter.ToString(data);
            }
        }

        protected string RemoveInvalidXMLChars(string content)
        {
            try
            {
                System.Xml.XmlConvert.VerifyXmlChars(content);
                return content;
            }
            catch
            {
                return new string(content.Where(ch => System.Xml.XmlConvert.IsXmlChar(ch)).ToArray());
            }
        }
        #endregion

        #region Static Methods
        public static bool IsSupported(FileInfo fi)
        {
            return CodecFactory.Instance.GetSupportedFileExtensions().Contains(fi.Extension.ToLower().Replace(".", string.Empty));
        }

        #endregion
    }
}
