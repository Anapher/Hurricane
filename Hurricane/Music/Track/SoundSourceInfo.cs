using System;
using CSCore;

namespace Hurricane.Music.Track
{
    public class SoundSourceInfo
    {
        public int kHz { get; set; }
        public TimeSpan Duration { get; set; }

        public static SoundSourceInfo FromSoundSource(IWaveSource source)
        {
            return new SoundSourceInfo { kHz = source.WaveFormat.SampleRate / 1000, Duration = source.GetLength() };
        }
    }
}
