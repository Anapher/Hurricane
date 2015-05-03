using System;

namespace Hurricane.Model.MusicEqualizer
{
    /// <summary>
    /// Provides data for the <see cref="E:Hurricane.Model.MusicEqualizer.EqualizerBandCollection.EqualizerBandChanged"/> event
    /// </summary>
    public class EqualizerBandChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The index (0-9) of the equalizer band
        /// </summary>
        public int BandIndex { get; set; }

        /// <summary>
        /// The new value (-100 to +100)
        /// </summary>
        public double NewValue { get; set; }

        /// <summary>
        /// The affected band
        /// </summary>
        public EqualizerBand EqualizerBand { get; set; }

        public EqualizerBandChangedEventArgs(int bandIndex, double newValue, EqualizerBand equalizerBand)
        {
            BandIndex = bandIndex;
            NewValue = newValue;
            EqualizerBand = equalizerBand;
        }
    }
}