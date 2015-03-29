using System;

namespace Hurricane.PluginAPI.AudioVisualisation
{
    /// <summary>
    /// Provides access to audio engine functionality needed to render the audio visualisation
    /// </summary>
    public interface ISpectrumProvider
    {
        /// <summary>
        /// Assigns current FFT data to a buffer.
        /// </summary>
        /// <param name="fftDataBuffer">The buffer to copy FFT data.</param>
        /// <returns>True if data was written to the buffer, otherwise false.</returns>
        bool GetFFTData(float[] fftDataBuffer);

        /// <summary>
        /// Gets the index in the FFT data buffer for a given frequency.
        /// </summary>
        /// <param name="frequency">The frequency for which to obtain a buffer index</param>
        /// <returns>An index in the FFT data buffer</returns>
        int GetFFTFrequencyIndex(int frequency);

        /// <summary>
        /// Gets whether the sound player is currently playing audio.
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// When <see cref="IsPlaying"/> is changed
        /// </summary>
        event EventHandler PlayStateChanged;
    }
}
