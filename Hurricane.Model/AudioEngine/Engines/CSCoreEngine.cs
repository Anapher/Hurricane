using System;
using System.Threading;
using System.Threading.Tasks;
using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.MP3;
using CSCore.SoundOut;
using CSCore.Streams;
using Hurricane.Model.MusicEqualizer;
using Hurricane.Utilities;
// ReSharper disable ExplicitCallerInfoArgument

namespace Hurricane.Model.AudioEngine.Engines
{
    // ReSharper disable once InconsistentNaming
    public class CSCoreEngine : PropertyChangedBase, IAudioEngine
    {
        const double MaxDb = 20;

        private float _volume = 1.0f;
        private ISoundOut _soundOut;
        private bool _isLoading;
        private IWaveSource _soundSource;
        private CancellationTokenSource _soundSourceLoadingToken;
        private SimpleNotificationSource _simpleNotificationSource;
        private EqualizerBandCollection _equalizerBands;
        private Equalizer _equalizer;
        private long _trackPosition;
        private CancellationTokenSource _soundSourcePositionToken;
        private TimeSpan _trackPositionTime;
        private readonly FadingService _fadingService;
        private bool _isPausing; //If its fading out
        private readonly CSCoreSoundOutProvider _soundOutProvider;

        public CSCoreEngine()
        {
            _fadingService = new FadingService();
            _soundOutProvider = new CSCoreSoundOutProvider();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (_fadingService.IsFading)
                    _fadingService.Cancel();
                StopPlayback();
                _soundOut?.Dispose();
                _soundOut = null;
                _soundSource?.Dispose();
                SoundOutProvider.Dispose();
            }
            // free native resources if there are any.
        }

        public event EventHandler<TrackFinishedEventArgs> TrackFinished;

        public long TrackPosition
        {
            get { return _soundSource == null ? 0 : _trackPosition; }
            set
            {
                if (_soundSource != null)
                {
                    _soundSourcePositionToken?.Cancel();
                    _soundSourcePositionToken = new CancellationTokenSource();
                    SetSoundSourcePosition(value, _soundSourcePositionToken.Token).Forget();
                    _trackPosition = value;
                    OnPositionChanged();
                }
            }
        }

        public long TrackLength => _soundSource?.Length ?? 0;

        public float Volume
        {
            get { return _volume; }
            set
            {
                if (SetProperty(value, ref _volume) && _soundOut != null)
                    _soundOut.Volume = value;
            }
        }

        public bool IsPlaying => _soundOut != null && _soundOut.PlaybackState == PlaybackState.Playing && !_isPausing;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(value, ref _isLoading); }
        }

        public bool IsLooping { get; set; }

        public TimeSpan TrackPositionTime
        {
            get { return _soundSource == null ? TimeSpan.Zero : _trackPositionTime; }
            private set
            {
                if ((int) value.TotalSeconds != (int) _trackPositionTime.TotalSeconds)
                    SetProperty(value, ref _trackPositionTime);
            }
        }

        public TimeSpan TrackLengthTime => _soundSource?.GetLength() ?? TimeSpan.Zero;

        public EqualizerBandCollection EqualizerBands
        {
            get { return _equalizerBands; }
            set
            {
                _equalizerBands = value;
                value.EqualizerBandChanged += EqualizerBandChanged;
            }
        }

        public ISoundOutProvider SoundOutProvider => _soundOutProvider;

        public async Task TogglePlayPause()
        {
            if (IsLoading || _soundSource == null)
                return;

            if(_fadingService.IsFading)
                _fadingService.Cancel();

            if (_soundOut.PlaybackState == PlaybackState.Playing)
            {
                _isPausing = true;
                CurrentStateChanged();
                await _fadingService.FadeOut(_soundOut, Volume);
                _soundOut?.Pause();
                _isPausing = false;
                CurrentStateChanged();
            }
            else
            {
                _soundOut?.Play();
                CurrentStateChanged();
                await _fadingService.FadeIn(_soundOut, Volume);
            }
        }

        public void StopAndReset()
        {
            
        }

        public async Task<bool> OpenTrack(IPlaySource track, bool openCrossfading, long position)
        {
            IsLoading = true;
            StopPlayback();

            if (_soundSource != null && !openCrossfading)
                _soundSource.Dispose();

            var tempSource = await GetSoundSource(track, position);
            if (tempSource == null)
                return false;
            _soundSource = tempSource;

            if (_soundSource.WaveFormat.SampleRate < 44100) //Correct sample rate
                _soundSource.ChangeSampleRate(44100);

            _soundSource = _soundSource
                .AppendSource(Equalizer.Create10BandEqualizer, out _equalizer)
                .AppendSource(x => new SimpleNotificationSource(x) {Interval = 100}, out _simpleNotificationSource)
                .ToWaveSource();

            _simpleNotificationSource.BlockRead += SimpleNotificationSource_BlockRead;

            for (var i = 0; i < EqualizerBands.Count; i++)
                SetEqualizerBandValue(EqualizerBands.Bands[i].Value, i);

            if (_soundOut == null)
            {
                _soundOut = _soundOutProvider.GetSoundOut();
            }
            _soundOut.Initialize(_soundSource);
            _soundOut.Volume = Volume;

            OnPropertyChanged(nameof(TrackLength));
            OnPropertyChanged(nameof(TrackLengthTime));

            CurrentStateChanged();
            IsLoading = false;
            return true;
        }

        private void SimpleNotificationSource_BlockRead(object sender, EventArgs e)
        {
            _trackPosition = _soundSource.Position;
            OnPositionChanged();
        }

        private void EqualizerBandChanged(object sender, EqualizerBandChangedEventArgs e)
        {
            SetEqualizerBandValue(e.NewValue, e.BandIndex);
        }

        private void SetEqualizerBandValue(double value, int band)
        {
            if (_equalizer == null)
                return;

            var newValue = (float) ((value/100)*MaxDb);
            _equalizer.SampleFilters[band].AverageGainDB = newValue;
        }

        private void StopPlayback()
        {
            if (_soundOut != null &&
                (_soundOut.PlaybackState == PlaybackState.Playing || _soundOut.PlaybackState == PlaybackState.Paused))
            {
                _soundOut.Stop();
                CurrentStateChanged();
            }
        }

        private async Task SetSoundSourcePosition(long value, CancellationToken token)
        {
            IsLoading = true;
            try
            {
                await Task.Run(() => _soundSource.Position = value, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            IsLoading = false;
        }

        private async Task<IWaveSource> GetSoundSource(IPlaySource track, long position)
        {
            _soundSourceLoadingToken?.Cancel();
            _soundSourceLoadingToken = new CancellationTokenSource();
            var token = _soundSourceLoadingToken.Token;
            IWaveSource result = null;

            try
            {
                switch (track.Type)
                {
                    case PlaySourceType.LocalFile:
                        result =
                            await
                                Task.Run(() => CodecFactory.Instance.GetCodec(((LocalFilePlaySource) track).Path), token);
                        break;
                    case PlaySourceType.Http:
                        result =
                            await Task.Run(() => CodecFactory.Instance.GetCodec(((HttpPlaySource) track).WebUri), token);
                        break;
                    case PlaySourceType.Stream:
                        result = new DmoMp3Decoder(((StreamPlaySource) track).Stream);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // ReSharper disable once AccessToDisposedClosure
                await Task.Run(() => result.Position = position, token);
            }
            catch (TaskCanceledException)
            {
                result?.Dispose();
                return null;
            }

            return token.IsCancellationRequested ? null : result;
        }

        protected void OnPositionChanged()
        {
            if (_soundSource == null)
                return;
            TrackPositionTime = TimeSpan.FromMilliseconds(_soundSource.WaveFormat.BytesToMilliseconds(TrackPosition));
            OnPropertyChanged(nameof(TrackPosition));
        }

        protected void CurrentStateChanged()
        {
            OnPropertyChanged(nameof(IsPlaying));
        }
    }
}