using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Hurricane.Model.MusicEqualizer;

namespace Hurricane.Model.AudioEngine
{
    /// <summary>
    /// Defines methods, functions and properties for an andio engine
    /// </summary>
    public interface IAudioEngine : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// When the track is naturally finished
        /// </summary>
        event EventHandler<TrackFinishedEventArgs> TrackFinished;

        /// <summary>
        /// Opens a track (but don't play it)
        /// </summary>
        /// <param name="track">The track to open</param>
        /// <param name="openCrossfading">If true, the current track fades out and the new one fades in</param>
        /// <param name="position">The start position. Is relative to <see cref="TrackLength"/></param>
        /// <returns>Returns if the <see cref="track"/> could be successfully opened</returns>
        Task<bool> OpenTrack(IPlaySource track, bool openCrossfading, long position);

        /// <summary>
        /// The position of current track, wayne in which format
        /// </summary>
        long TrackPosition { get; set; }

        /// <summary>
        /// The length of the current track, should be relative to the <see cref="TrackPosition"/>
        /// </summary>
        long TrackLength { get; }

        /// <summary>
        /// The volume of the playback
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// Returns if it is currently playing
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Returns if it is currently loading
        /// </summary>
        bool IsLoading { get; }

        /// <summary>
        /// If set to true, the audio engine should set the position to 0 and continue playing instead of firing the <see cref="TrackFinished" /> event
        /// </summary>
        bool IsLooping { set; }

        /// <summary>
        /// The position (as <see cref="TimeSpan"/>) of the current track
        /// </summary>
        TimeSpan TrackPositionTime { get; }

        /// <summary>
        /// The length (as <see cref="TimeSpan"/>) of the current track
        /// </summary>
        TimeSpan TrackLengthTime { get; }

        /// <summary>
        /// Gets or sets the equalizer bands. <see cref="Hurricane.Model.MusicEqualizer.EqualizerBandCollection.EqualizerBandChanged"/> should be subscribed
        /// </summary>
        EqualizerBandCollection EqualizerBands { get; set; }

        /// <summary>
        /// The sound out provider
        /// </summary>
        ISoundOutProvider SoundOutProvider { get; }

        /// <summary>
        /// Toggles play/pause
        /// </summary>
        Task TogglePlayPause();

        /// <summary>
        /// Stops the current track and resets everything
        /// </summary>
        void StopAndReset();
    }
}