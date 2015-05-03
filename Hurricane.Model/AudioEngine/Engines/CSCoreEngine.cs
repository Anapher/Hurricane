using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore.SoundOut;
using Hurricane.Model.MusicEqualizer;

namespace Hurricane.Model.AudioEngine.Engines
{
    // ReSharper disable once InconsistentNaming
    public class CSCoreEngine : PropertyChangedBase, IAudioEngine
    {
        private float _volume = 1.0f;
        private ISoundOut _soundOut;
        private bool _isLoading;

        public CSCoreEngine()
        {
            
        }

        public void Dispose()
        {
            
        }

        public event EventHandler<TrackFinishedEventArgs> TrackFinished;

        public long TrackPosition
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public long TrackLength
        {
            get { throw new NotImplementedException(); }
        }

        public float Volume
        {
            get { return _volume; }
            set
            {
                if (SetProperty(value, ref _volume))
                    _soundOut.Volume = value;
            }
        }

        public bool IsPlaying
        {
            get { return _soundOut != null && _soundOut.PlaybackState == PlaybackState.Playing; }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(value, ref _isLoading); }
        }

        public TimeSpan TrackPositionTime
        {
            get { throw new NotImplementedException(); }
        }

        public TimeSpan TrackLengthTime
        {
            get { throw new NotImplementedException(); }
        }

        public EqualizerBandCollection EqualizerBands
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void TogglePlayPause()
        {
            throw new NotImplementedException();
        }

        public void StopAndReset()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> OpenTrack(IPlaySource track, bool openCrossfading)
        {
            IsLoading = true;
            StopPlayback();
            return true;
        }

        private void StopPlayback()
        {
            if (_soundOut != null && (_soundOut.PlaybackState == PlaybackState.Playing || _soundOut.PlaybackState == PlaybackState.Paused))
                _soundOut.Stop();
        }
    }
}