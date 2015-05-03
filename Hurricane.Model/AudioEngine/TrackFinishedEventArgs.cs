using System;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.AudioEngine
{
    /// <summary>
    /// Provides data for the <see cref="E:Hurricane.Model.AudioEngine.IAudioEngine.TrackFinished"/> event
    /// </summary>
    public class TrackFinishedEventArgs : EventArgs
    {
        /// <summary>
        /// The finished playable
        /// </summary>
        public IPlayable CurrentPlayable { get; set; }

        public TrackFinishedEventArgs(IPlayable currentPlayable)
        {
            CurrentPlayable = currentPlayable;
        }
    }
}