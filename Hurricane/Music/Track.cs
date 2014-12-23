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
using System.Windows.Media.Imaging;

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
        public string Genres { get; set; }
        public uint Year { get; set; }
        public DateTime TimeAdded { get; set; }
        public DateTime LastTimePlayed { get; set; }
        
        private String album;
        public String Album
        {
            get { return album; }
            set
            {
                SetProperty(value, ref album);
            }
        }

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

        
        private BitmapImage image;
        [XmlIgnore]
        public BitmapImage Image
        {
            get { return image; }
            set
            {
                SetProperty(value, ref image);
            }
        }

        private bool isloadingimage;
        [XmlIgnore]
        public bool IsLoadingImage
        {
            get { return isloadingimage; }
            set
            {
                SetProperty(value, ref isloadingimage);
            }
        }

        public TimeSpan DurationTimespan
        {
            get
            {
                if (Duration.Split(':').Length == 2)
                {
                    return TimeSpan.ParseExact(Duration, @"mm\:ss", null);

                }
                else
                {
                    return TimeSpan.ParseExact(Duration, @"hh\:mm\:ss", null);
                }
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

            try //We just try to open the file to test if it works with CSCore
            {
                using (IWaveSource SoundSource = CodecFactory.Instance.GetCodec(Path))
                {
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

            try
            {
                using (TagLib.File info = TagLib.File.Create(file.FullName))
                {
                    if (!string.IsNullOrWhiteSpace(info.Tag.FirstPerformer))
                    {
                        this.Artist = RemoveInvalidXMLChars(info.Tag.FirstPerformer);
                    }
                    else
                    {
                        this.Artist = RemoveInvalidXMLChars(info.Tag.FirstAlbumArtist);
                    }
                    if (!string.IsNullOrWhiteSpace(info.Tag.Title))
                    {
                        this.Title = RemoveInvalidXMLChars(info.Tag.Title);
                    }
                    else
                    {
                        this.Title = System.IO.Path.GetFileNameWithoutExtension(file.FullName);
                    }
                    this.Genres = info.Tag.JoinedGenres;
                    this.kbps = info.Properties.AudioBitrate;
                    this.kHz = info.Properties.AudioSampleRate / 1000;
                    this.Extension = file.Extension.ToUpper().Replace(".", string.Empty);
                    this.Year = info.Tag.Year;
                }
            }
            catch (NullReferenceException)
            {
                this.Title = System.IO.Path.GetFileNameWithoutExtension(file.FullName);
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
        public async void Load()
        {
            this.IsLoadingImage = true;
            try
            {
                using (TagLib.File file = TagLib.File.Create(Path))
                {
                    if (file.Tag.Pictures != null && file.Tag.Pictures.Any())
                    {
                        this.Image = Utilities.GeneralHelper.ByteArrayToBitmapImage(file.Tag.Pictures.First().Data.ToArray());
                    }
                }
            }
            catch (Exception)
            {
                this.Image = null;
            }

            if (this.Image == null)
            {
                DirectoryInfo diAlbumCover = new DirectoryInfo("AlbumCover");
                this.Image = MusicDatabase.MusicCoverManager.GetImage(this, diAlbumCover);
                if (this.Image == null && Settings.HurricaneSettings.Instance.Config.LoadAlbumCoverFromInternet && Utilities.GeneralHelper.CheckForInternetConnection())
                {
                    try
                    {
                        this.Image = await MusicDatabase.MusicCoverManager.LoadCoverFromWeb(this, diAlbumCover).ConfigureAwait(false);
                    }
                    catch (System.Net.WebException)
                    {
                        //Happens, doesn't matter
                    }
                }
            }
            this.IsLoadingImage = false;
        }

        /// <summary>
        /// Delete all which was needed for playing
        /// </summary>
        public void Unload()
        {
            if (this.Image != null)
            {
                if (this.Image.StreamSource != null) this.Image.StreamSource.Dispose();
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
            if (string.IsNullOrEmpty(content)) return string.Empty;
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
