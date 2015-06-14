using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Hurricane.Model.AudioEngine;
using Hurricane.Model.AudioEngine.Engines;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.MusicEqualizer;
using Hurricane.Utilities;

namespace Hurricane.Model.Music
{
    public class MusicManager : PropertyChangedBase, IDisposable
    {
        private IPlayable _currentTrack;
        private IPlaylist _currentPlaylist;
        private PlayMode _currentPlayMode;
        private bool _isCrossfadeEnabled;
        private readonly List<Tuple<IPlaylist, IPlayable>> _tempHistory;

        public MusicManager()
        {
            _tempHistory = new List<Tuple<IPlaylist, IPlayable>>();
            AudioEngine = new CSCoreEngine();
            AudioEngine.TrackFinished += AudioEngineOnTrackFinished;
            AudioEngine.EqualizerBands = new EqualizerBandCollection();
            TrackHistory = new ObservableCollection<IPlayable>();
            Queue = new Queue();

            AudioEngine.CrossfadeDuration = TimeSpan.FromSeconds(4);
            IsCrossfadeEnabled = true;
        }

        private void AudioEngine_TrackPositionChanged(object sender, EventArgs e)
        {
            if (AudioEngine.TrackPositionTime.TotalSeconds >
                (AudioEngine.TrackLengthTime.TotalSeconds - AudioEngine.CrossfadeDuration.TotalSeconds))
                GoForward(true);
        }

        public void Dispose()
        {
            AudioEngine.Dispose();
        }

        public event EventHandler QueuePlaying;

        public IPlayable CurrentTrack
        {
            get { return _currentTrack; }
            set
            {
                var oldValue = _currentTrack;
                if (SetProperty(value, ref _currentTrack))
                {
                    value.IsPlaying = true;
                    if (oldValue != null)
                        oldValue.IsPlaying = false;
                }
            }
        }

        public IPlaylist CurrentPlaylist
        {
            get { return _currentPlaylist; }
            set { SetProperty(value, ref _currentPlaylist); }
        }

        public PlayMode CurrentPlayMode
        {
            get { return _currentPlayMode; }
            set
            {
                if (value == PlayMode.Loop || _currentPlayMode == PlayMode.Loop)
                    AudioEngine.IsLooping = value == PlayMode.Loop;
                SetProperty(value, ref _currentPlayMode);
            }
        }

        public bool IsCrossfadeEnabled
        {
            get { return _isCrossfadeEnabled; }
            set
            {
                if (SetProperty(value, ref _isCrossfadeEnabled))
                {
                    if (value)
                        AudioEngine.TrackPositionChanged += AudioEngine_TrackPositionChanged;
                    else AudioEngine.TrackPositionChanged -= AudioEngine_TrackPositionChanged;
                }
            }
        }

        public IAudioEngine AudioEngine { get; }
        public ObservableCollection<IPlayable> TrackHistory { get; }
        public Queue Queue { get; }

        private async Task OpenPlayable(IPlayable playable, IPlaylist playlist, bool openPlaying, bool openCrossfading, bool addToTempHistory)
        {
            CurrentTrack = playable;
            CurrentPlaylist = playlist;

            if (await AudioEngine.OpenTrack(await playable.GetSoundSource(), IsCrossfadeEnabled && openCrossfading, 0))
            {
                var track = playable as PlayableBase;
                if (track != null)
                    track.LastTimePlayed = DateTime.Now;
                
                TrackHistory.Add(playable);
                playlist?.GetBackHistory().Add(track);
                if (addToTempHistory && (_tempHistory.Count == 0 || _tempHistory.Last().Item1 != playlist || _tempHistory.Last().Item2 != playable))
                    _tempHistory.Add(Tuple.Create(playlist, playable));

                if (openPlaying && !(IsCrossfadeEnabled && openCrossfading))
                    await AudioEngine.TogglePlayPause();
            }
        }

        public Task OpenPlayable(IPlayable playable, IPlaylist playlist)
        {
            return OpenPlayable(playable, playlist, true, false, true);
        }

        public Task OpenPlayable(IPlayable playable, IPlaylist playlist, bool openPlaying)
        {
            return OpenPlayable(playable, playlist, openPlaying, false, true);
        }

        public void GoForward(bool crossfade)
        {
            IPlayable track;

            if (Queue.Playables.Any())
            {
                track = Queue.GetNextPlayable();
                QueuePlaying?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                if (CurrentPlaylist == null || CurrentPlaylist.Tracks.Count == 0 ||
                    CurrentPlaylist.Tracks.All(x => !x.IsAvailable))
                    return;

                track = CurrentPlayMode == PlayMode.Default
                    ? CurrentPlaylist.GetNextTrack(CurrentTrack)
                    : CurrentPlaylist.GetRandomTrack();
            }

            OpenPlayable(track, CurrentPlaylist, true, crossfade, true).Forget();
        }

        public void GoForward()
        {
            GoForward(false);
        }

        public void GoBack()
        {
            if (CurrentPlaylist == null || CurrentPlaylist.Tracks.Count == 0 ||
                CurrentPlaylist.Tracks.All(x => !x.IsAvailable))
                return;

            if (CurrentTrack == null)
            {
                OpenPlayable(CurrentPlaylist.Tracks.Last(), CurrentPlaylist).Forget();
                return;
            }

            if (_tempHistory.Count > 1) //Check if there are more than two tracks, because the current track is the last one in the list
            {
                _tempHistory.RemoveAt(_tempHistory.Count - 1); //This is the current track
                var newTrack = _tempHistory.Last();
                OpenPlayable(newTrack.Item2, newTrack.Item1, true, false, false).Forget();
            }
            else
            {
                OpenPlayable(CurrentPlaylist.GetPreviousTrack(CurrentTrack), CurrentPlaylist, true, false, false)
                    .Forget();
            }
        }

        private void AudioEngineOnTrackFinished(object sender, EventArgs e)
        {
            switch (CurrentPlayMode)
            {
                case PlayMode.Default:
                case PlayMode.Shuffle:
                    GoForward();
                    break;
                case PlayMode.Loop:
                    throw new InvalidOperationException("Why the hell fire the trackfinished event, if loop is enabled?! EPIC FAIL");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum PlayMode
    {
        Default,
        Shuffle,
        Loop
    }
}