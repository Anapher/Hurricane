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

namespace Hurricane.Music
{
    [Serializable]
   public class Track : ViewModelBase.PropertyChangedBase
    {
        public string Duration { get; set; }
        public int kHz { get; set; }
        public int kbps { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public string Artist { get; set; }
        public string Extension { get; set; }
        [System.Xml.Serialization.XmlIgnore]
        public System.Drawing.Image Image { get; set; }

        public DateTime TimeAdded { get; set; }
        public DateTime LastTimePlayed { get; set; }

        private bool isplaying;
        [System.Xml.Serialization.XmlIgnore]
        public bool IsPlaying
        {
            get { return isplaying; }
            set
            {
                SetProperty(value, ref isplaying);
            }
        }

        private bool isselected;
        public bool IsSelected
        {
            get { return isselected; }
            set
            {
                SetProperty(value, ref isselected);
            }
        }

        public TimeSpan DurationTimespan
        {
            get
            {
                return TimeSpan.Parse(Duration);
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

        [System.Xml.Serialization.XmlIgnore]
        public ID3v2QuickInfo TagInfo { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public FileInfo TrackInformations
        {
            get
            {
                return new FileInfo(Path);
            }
        }

        public void LoadInformations()
        {
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
                    this.kbps = frame.BitRate / 1000;
                }
                this.Extension = file.Extension.ToUpper().Replace(".", string.Empty);
                try
                {
                    using (IWaveSource SoundSource = CodecFactory.Instance.GetCodec(Path))
                    {
                        this.kHz = SoundSource.WaveFormat.SampleRate / 1000;
                        TimeSpan duration =  SoundSource.GetLength();
                        
                        if(duration.Hours == 0){
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

                }
        }

        public void Unload()
        {
            if (this.Image != null)
            {
                this.Image.Dispose();
                this.Image = null;
            }
        }

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

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Artist))
                return string.Format("{0} - {1}", this.Artist, this.Title);

            return Title;
        }

        public bool CheckFile()
        {
            return TrackInformations.Exists;
        }

        public static bool IsSupported(FileInfo fi)
        {
            return CodecFactory.Instance.GetSupportedFileExtensions().Contains(fi.Extension.ToLower().Replace(".", string.Empty));
        }
    }
}
