using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.MP3;
using Hurricane.Music.MusicDatabase;
using Hurricane.Settings;
using Hurricane.Utilities;
using File = TagLib.File;

namespace Hurricane.Music.Track
{
    public class LocalTrack : PlayableBase
    {
        public string Path { get; set; }
        public string Extension { get; set; }

        [XmlElement("IsChecked")]
        public string Checked { get; set; }

        [XmlIgnore]
        public bool IsChecked
        {
            get { return Checked == "1"; }
            set { Checked = value ? "1" : "0"; }
        }

        public bool ShouldSerializeChecked()
        {
            return !IsChecked;
        }

        public async Task<bool> CheckTrack()
        {
            TimeSpan duration = TimeSpan.Zero;
            try
            {
                await Task.Run(() =>
                {
                    using (IWaveSource soundSource = CodecFactory.Instance.GetCodec(Path))
                    {
                        duration = soundSource.GetLength();
                    }
                });
                SetDuration(duration);
            }
            catch (Exception)
            {
                return false;
            }

            IsChecked = true;
            return true;
        }

        private FileInfo _trackinformation;
        public FileInfo TrackInformation
        {
            get { return _trackinformation ?? (_trackinformation = new FileInfo(Path)); }
        }

        #region Information loading

        public async override Task<bool> LoadInformation()
        {
            _trackinformation = null; //to refresh the fileinfo
            FileInfo file = TrackInformation;
            IsChecked = false;
            Extension = file.Extension.ToUpper().Replace(".", string.Empty);

            return await TryLoadWithTagLibSharp(file) || await TryLoadWithCSCore(file);
        }

        private async Task<bool> TryLoadWithTagLibSharp(FileInfo filename)
        {
            File info = null;

            try
            {
                await Task.Run(() => info = File.Create(filename.FullName));
            }
            catch (Exception)
            {
                return false;
            }

            using (info)
            {
                Artist = RemoveInvalidXmlChars(!string.IsNullOrWhiteSpace(info.Tag.FirstPerformer) ? info.Tag.FirstPerformer : info.Tag.FirstAlbumArtist);
                Title = !string.IsNullOrWhiteSpace(info.Tag.Title) ? RemoveInvalidXmlChars(info.Tag.Title) : System.IO.Path.GetFileNameWithoutExtension(filename.FullName);
                Album = RemoveInvalidXmlChars(info.Tag.Album);
                Genres = string.Join(", ", info.Tag.Genres);
                kbps = info.Properties.AudioBitrate;
                kHz = info.Properties.AudioSampleRate / 1000;
                Year = info.Tag.Year;
                SetDuration(info.Properties.Duration);
            }
            return true;
        }

        private async Task<bool> TryLoadWithCSCore(FileInfo filename)
        {
            this.Title = System.IO.Path.GetFileNameWithoutExtension(filename.FullName);
            Mp3Frame frame = null;

            try
            {
                await Task.Run(() =>
                {
                    using (FileStream sr = new FileStream(filename.FullName, FileMode.Open, FileAccess.Read))
                        frame = Mp3Frame.FromStream(sr);
                });
                
                if (frame != null) { kbps = frame.BitRate / 1000; }
                TimeSpan duration = TimeSpan.Zero;
                int samplerate = 0;

                await Task.Run(() =>
                {
                    using (var SoundSource = CodecFactory.Instance.GetCodec(filename.FullName))
                    {
                        samplerate = SoundSource.WaveFormat.SampleRate;
                        duration = SoundSource.GetLength();
                    }
                });

                kHz = samplerate / 1000;
                SetDuration(duration);
                IsChecked = true;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        protected string RemoveInvalidXmlChars(string content)
        {
            if (string.IsNullOrEmpty(content)) return string.Empty;
            try
            {
                XmlConvert.VerifyXmlChars(content);
                return content;
            }
            catch { return new string(content.Where(XmlConvert.IsXmlChar).ToArray()); }
        }

        #endregion

        public override bool TrackExists
        {
            get { return TrackInformation.Exists; }
        }

        #region Image
        public override async void Load()
        {
            IsLoadingImage = true;
            try
            {
                using (File file = File.Create(Path))
                {
                    if (file.Tag.Pictures != null && file.Tag.Pictures.Any())
                    { Image = GeneralHelper.ByteArrayToBitmapImage(file.Tag.Pictures.First().Data.ToArray()); }
                }
            }
            catch (Exception)
            {
                Image = null;
            }

            if (Image == null)
            {
                DirectoryInfo diAlbumCover = new DirectoryInfo(HurricaneSettings.Instance.CoverDirectory);
                Image = MusicCoverManager.GetImage(this, diAlbumCover);
                if (Image == null && HurricaneSettings.Instance.Config.LoadAlbumCoverFromInternet)
                {
                    try
                    {
                        Image = await MusicCoverManager.LoadCoverFromWeb(this, diAlbumCover).ConfigureAwait(false);
                    }
                    catch (WebException)
                    {
                        //Happens, doesn't matter
                    }
                }
            }
            IsLoadingImage = false;
            OnImageLoadComplete();
        }

        #endregion

        #region Static Methods

        public static bool IsSupported(FileInfo fi)
        {
            return CodecFactory.Instance.GetSupportedFileExtensions().Contains(fi.Extension.ToLower().Replace(".", string.Empty));
        }

        #endregion

        public override void RefreshTrackExists()
        {
            _trackinformation = null;
            base.RefreshTrackExists();
        }

        public override Task<IWaveSource> GetSoundSource()
        {
            return Task.Run(() => CodecFactory.Instance.GetCodec(Path));
        }

        private string _FileHash;

        public override bool Equals(PlayableBase other)
        {
            if (other == null) return false;
            if (!other.TrackExists || !TrackExists) return false;
            if (GetType() != other.GetType()) return false;

            var otherAsLocalTrack = (LocalTrack) other;
            if (TrackInformation.FullName == otherAsLocalTrack.TrackInformation.FullName) return true;

            if (TrackInformation.Length == otherAsLocalTrack.TrackInformation.Length)
            {
                if (string.IsNullOrEmpty(_FileHash))
                    _FileHash = GeneralHelper.FileToMD5Hash(TrackInformation.FullName);
                if (string.IsNullOrEmpty(otherAsLocalTrack._FileHash))
                    otherAsLocalTrack._FileHash = GeneralHelper.FileToMD5Hash(TrackInformation.FullName);
                if (otherAsLocalTrack._FileHash == _FileHash) return true;
            }
            return false;
        }

        public override void OpenTrackLocation()
        {
            Process.Start("explorer.exe", "/select, \"" + Path + "\"");
        }

        public override TrackType TrackType
        {
            get { return TrackType.File; }
        }
    }
}
