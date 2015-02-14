using System;
using CSCore;

namespace Hurricane.Music
{
    class CutSource : WaveAggregatorBase
    {
        public CutSource(IWaveSource source, TimeSpan startPosition, TimeSpan trackLength)
            : base(source)
        {
            StartPosition = startPosition;
            TrackLength = trackLength;
        }

        public TimeSpan StartPosition { get; set; }
        public TimeSpan TrackLength { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var position = TimeSpan.FromMilliseconds(this.GetMilliseconds(base.Position));
            if (position.Seconds < StartPosition.Seconds) Position = 0;
            if (position.Seconds > StartPosition.Seconds + TrackLength.Seconds) return 0;

            return base.Read(buffer, offset, count);
        }

        public override long Length
        {
            get { return this.GetBytes(TrackLength); }
        }

        public override long Position
        {
            get { return base.Position - this.GetBytes(StartPosition); }
            set { base.Position = value + this.GetBytes(StartPosition); }
        }
    }
}
