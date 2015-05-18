using System;
using System.Linq;
using System.Threading.Tasks;
using Hurricane.Model.AudioEngine;
using Hurricane.Model.AudioEngine.Engines;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;

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
        }

        public void Dispose()
        {
            AudioEngine.Dispose();
        }

        public IPlayable CurrentTrack
        {
            get { return _currentTrack; }
            set { SetProperty(value, ref _currentTrack); }
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

        public async Task OpenPlayable(IPlayable playable, IPlaylist playlist, bool openPlaying)
        {
            CurrentTrack = playable;
            CurrentPlaylist = playlist;
            if (await AudioEngine.OpenTrack(await playable.GetSoundSource(), IsCrossfadeEnabled) && openPlaying)
                AudioEngine.TogglePlayPause();
        }

        public void GoNext()
        {
            
        }

        public void GoBack()
        {
            
        }

        private void AudioEngineOnTrackFinished(object sender, TrackFinishedEventArgs trackFinishedEventArgs)
        {
            switch (CurrentPlayMode)
            {
                case PlayMode.Default:
                case PlayMode.Shuffle:
                    GoNext();
                    break;
                case PlayMode.Loop:
                    throw new InvalidOperationException("Why the hell fire the trackfinished event if loop is enabled?!");
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