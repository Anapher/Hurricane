using System;
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

        public MusicManager()
        {
            AudioEngine = new CSCoreEngine();
            AudioEngine.TrackFinished += AudioEngineOnTrackFinished;
            AudioEngine.EqualizerBands = new EqualizerBandCollection();
            TrackHistory = new ObservableCollection<IPlayable>();

            AudioEngine.CrossfadeDuration = TimeSpan.FromSeconds(4);
        }

        public void Dispose()
        {
            AudioEngine.Dispose();
        }

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
            set { SetProperty(value, ref _isCrossfadeEnabled); }
        }

        public IAudioEngine AudioEngine { get; }
        public ObservableCollection<IPlayable> TrackHistory { get; }

        public async Task OpenPlayable(IPlayable playable, IPlaylist playlist, bool openPlaying)
        {
            CurrentTrack = playable;
            CurrentPlaylist = playlist;
            if (await AudioEngine.OpenTrack(await playable.GetSoundSource(), true, 0) && openPlaying)
            {
                var track = playable as PlayableBase;
                if (track != null)
                {
                    track.LastTimePlayed = DateTime.Now;
                }
                TrackHistory.Add(playable);
                await AudioEngine.TogglePlayPause();
            }
        }

        public void GoForward()
        {
            if (CurrentPlaylist == null || CurrentPlaylist.Tracks.Count == 0 || CurrentPlaylist.Tracks.All(x => !x.IsAvailable)) return;
            var track = CurrentPlayMode == PlayMode.Default ? CurrentPlaylist.GetNextTrack(CurrentTrack) : CurrentPlaylist.GetRandomTrack();
            if(CurrentPlaylist.History.Count(x => CurrentPlaylist.Tracks.Contains(x)) == CurrentPlaylist.Tracks.Count)
                CurrentPlaylist.History.Clear();

            CurrentPlaylist.History.Add(track);
            OpenPlayable(track, CurrentPlaylist, true).Forget();
        }

        public void GoBack()
        {

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