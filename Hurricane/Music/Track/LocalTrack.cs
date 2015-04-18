using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.MP3;
using Hurricane.Music.MusicCover;
using Hurricane.Settings;
using Hurricane.Utilities;
using File = TagLib.File;

namespace Hurricane.Music.Track
{
    public class LocalTrack : PlayableBase
    {
        public string Path { get; set; }
        public string Extension { get; set; }

        private FileInfo _trackinformation;
        public FileInfo TrackInformation
        {
            get { return _trackinformation ?? (_trackinformation = new FileInfo(Path)); }
        }

        #region Information loading

        public async override Task<bool> LoadInformation()
        {
            _trackinformation = null; //to refresh the fileinfo
            Extension = TrackInformation.Extension.ToUpper().Replace(".", string.Empty);

            return await UpdateInformation(TrackInformation);
        }

        protected virtual async Task<bool> UpdateInformation(FileInfo filename)
        {
            return await TryLoadWithTagLibSharp(TrackInformation) || await TryLoadWithCSCore(TrackInformation);
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
                Genres = new List<Genre>(info.Tag.Genres.Select(StringToGenre));

                if (info.Properties.AudioBitrate > 56000) //No idea what TagLib# is thinking, but sometimes it shows the bitrate * 1000
                {
                    kbps = (int)Math.Round(info.Properties.AudioBitrate / (double)1000, 0);
                }
                else
                {
                    kbps = info.Properties.AudioBitrate;
                }
                kHz = info.Properties.AudioSampleRate / 1000;
                Year = info.Tag.Year;
                SetDuration(info.Properties.Duration);
            }
            return true;
        }

        private async Task<bool> TryLoadWithCSCore(FileInfo filename)
        {
            Title = System.IO.Path.GetFileNameWithoutExtension(filename.FullName);
            Mp3Frame frame = null;

            try
            {
                await Task.Run(() =>
                {
                    using (FileStream sr = new FileStream(filename.FullName, FileMode.Open, FileAccess.Read))
                        frame = Mp3Frame.FromStream(sr);
                });

                if (frame != null) { kbps = (int)Math.Round(frame.BitRate / (double)1000, 0); }
                TimeSpan duration = TimeSpan.Zero;
                int samplerate = 0;

                await Task.Run(() =>
                {
                    using (var soundSource = CodecFactory.Instance.GetCodec(filename.FullName))
                    {
                        samplerate = soundSource.WaveFormat.SampleRate;
                        duration = soundSource.GetLength();
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

        public void ResetDuration(TimeSpan timeSpan)
        {
            SetDuration(timeSpan);
        }

        public override bool TrackExists
        {
            get { return System.IO.File.Exists(Path); }
        }

        #region Image

        protected async override Task LoadImage(DirectoryInfo albumCoverDirectory)
        {
            try
            {
                using (var file = File.Create(Path))
                {
                    if (file.Tag.Pictures != null && file.Tag.Pictures.Any())
                    {
                        Image = ImageHelper.ByteArrayToBitmapImage(file.Tag.Pictures.First().Data.ToArray());
                        return;
                    }
                }
            }
            catch
            {
                // ignored
            }

            Image = MusicCoverManager.GetAlbumImage(this, albumCoverDirectory);
            if (Image != null) return;

            if (HurricaneSettings.Instance.Config.LoadAlbumCoverFromInternet)
            {
                try
                {
                    Image = await MusicCoverManager.LoadCoverFromWeb(this, albumCoverDirectory);
                }
                catch (WebException)
                {
                    //Happens, doesn't matter
                }
            }
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
            return Task.Run(() => CutWaveSource(CodecFactory.Instance.GetCodec(Path)));
        }

        public virtual string UniqueId
        {
            get { return TrackInformation.FullName; }
        }

        public override bool Equals(PlayableBase other)
        {
            if (other == null) return false;
            if (!other.TrackExists || !TrackExists) return false;
            if (GetType() != other.GetType()) return false;

            var otherAsLocalTrack = (LocalTrack)other;

            if (UniqueId == otherAsLocalTrack.UniqueId) return true;
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
