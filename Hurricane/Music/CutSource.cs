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

            if (position < StartPosition)
                Position = 0;

            if (position > StartPosition + TrackLength)
                return 0;

            return base.Read(buffer, offset, count);
        }

        public override long Length
        {
            get { return this.GetBytes(TrackLength); }
        }

        public override long Position
        {
            get { return base.Position - this.GetBytes(StartPosition); }
            set {
                var position = value + this.GetBytes(StartPosition);
                if (value < 0 || position < 0)
                    throw new ArgumentOutOfRangeException("Invalid Position");
                base.Position = position;
            }
        }
    }
}
