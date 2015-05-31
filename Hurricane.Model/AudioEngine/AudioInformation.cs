using System;

namespace Hurricane.Model.AudioEngine
{
    /// <summary>
    /// Audio information gathered by the <see cref="IAudioEngine.TestAudioFile"/> method.
    /// </summary>
    public sealed class AudioInformation
    {
        /// <summary>
        /// The sample rate of the audio file in Hz
        /// </summary>
        public int SampleRate { get; set; }
        /// <summary>
        /// The duration of the audio file
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// The bitrate of the audio file (kbit/s)
        /// </summary>
        public int Bitrate { get; set; }
    }
}