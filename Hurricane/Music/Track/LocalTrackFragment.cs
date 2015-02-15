using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CSCore;
using CSCore.Codecs;

namespace Hurricane.Music.Track
{
    public class LocalTrackFragment : LocalTrack
    {
        LocalTrackFragment()
        { }

        public LocalTrackFragment(TimeSpan start_from, TimeSpan duration, string title)
        {
            Offset = start_from;
            _duration = duration;
            _title = title;
            Title = title;
            if (duration > TimeSpan.Zero)
                ResetDuration(duration);

            DurationTicks = duration.Ticks;
            OffsetTicks = Offset.Ticks;
        }

        protected override async Task<bool> UpdateInformation(FileInfo filename)
        {
            try
            {
                await Task.Run(() => {
                    using (var source = CodecFactory.Instance.GetCodec(filename.FullName))
                        UpdateMetadata(source);
                });

                IsChecked = true;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        void UpdateMetadata(IWaveSource source)
        {
            // duration of the last track imported from a CUE sheet is not initially known;
            // update it now that we have audio source decoded; this update is only valid for the last track!
            if (_duration == TimeSpan.Zero)
            {
                var duration = source.GetLength();
                _duration = duration - Offset;
                SetDuration(_duration);
            }

            kHz = source.WaveFormat.SampleRate / 1000;
            kbps = source.WaveFormat.BytesPerSecond * 8 / 1000;
        }

        // return fragment to play
        public override Task<IWaveSource> GetSoundSource()
        {
            return Task.Run(() => {
                var source = CodecFactory.Instance.GetCodec(Path);
                UpdateMetadata(source);
                return new CutSource(source, Offset, _duration) as IWaveSource;
            });
        }

        public override async Task<bool> CheckTrack()
        {
            if (!TrackExists)
                return false;
            IsChecked = true;
            return true;
        }

        public override string UniqueId
        {
            get { return string.Format("{0}-{1}", Path, TrackNumber); }
        }

        [XmlIgnore]
        TimeSpan Offset { get; set; }

        [XmlIgnore]
        TimeSpan _duration;

        [XmlElement("Offset")]
        public long OffsetTicks
        {
            get { return Offset.Ticks; }
            set { Offset = new TimeSpan(value); }
        }

        [XmlElement("DurationExact")]
        public long DurationTicks
        {
            get { return _duration.Ticks; }
            set { _duration = new TimeSpan(value); }
        }

        string _title;
    }
}
