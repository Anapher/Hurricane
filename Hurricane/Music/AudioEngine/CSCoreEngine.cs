using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AudioVisualisation;
using CSCore;
using CSCore.SoundOut;
using CSCore.Streams;
using Hurricane.Music.CustomEventArgs;
using Hurricane.Music.MusicEqualizer;
using Hurricane.Music.Track;
using Hurricane.Music.Visualization;
using Hurricane.Settings;
using Hurricane.ViewModelBase;
// ReSharper disable InconsistentNaming

namespace Hurricane.Music.AudioEngine
{
    public class CSCoreEngine : PropertyChangedBase, IDisposable, ISpectrumProvider
    {
        #region Consts
        const int FFTSize = 4096;
        const double MaxDB = 20;

        #endregion

        #region Events
        public event EventHandler StartVisualization;
        public event EventHandler TrackFinished;
        public event EventHandler<TrackChangedEventArgs> TrackChanged;
        public event EventHandler<PlayStateChangedEventArgs> PlaybackStateChanged;
        public event EventHandler<PositionChangedEventArgs> PositionChanged;
        public event EventHandler VolumeChanged;
        public event EventHandler<Exception> ExceptionOccurred;
        public event EventHandler PlayStateChanged;
        public event EventHandler<string> SoundOutErrorOccurred;

        #endregion

        #region Event handler
        protected void OnTrackFinished()
        {
            if (TrackFinished != null) TrackFinished(this, EventArgs.Empty);
        }

        protected void OnTrackChanged()
        {
            if (TrackChanged != null) TrackChanged(this, new TrackChangedEventArgs(CurrentTrack));
        }

        protected void OnSoundOutErrorOccurred(string message)
        {
            if (SoundOutErrorOccurred != null) SoundOutErrorOccurred(this, message);
        }

        #endregion

        #region Properties
        private float _volume = 1.0f;
        public float Volume
        {
            get { return _volume; }
            set
            {
                if (SetProperty(value, ref _volume))
                {
                    if (_soundOut != null && _soundOut.WaveSource != null)
                        _soundOut.Volume = value;
                    if (VolumeChanged != null) VolumeChanged(this, EventArgs.Empty);
                }
            }
        }

        public long Position
        {
            get
            {
                if (SoundSource == null) return 0;
                try
                {
                    return SoundSource.Position;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            set
            {
                if (SoundSource != null)
                {
                    try
                    {
                        SoundSource.Position = value;
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
                OnPropertyChanged("CurrentTrackPosition");
                if (PositionChanged != null) PositionChanged(this, new PositionChangedEventArgs((int)this.CurrentTrackPosition.TotalSeconds, (int)this.CurrentTrackLength.TotalSeconds));
            }
        }

        private PlayableBase _currenttrack;
        public PlayableBase CurrentTrack
        {
            get { return _currenttrack; }
            protected set
            {
                if (SetProperty(value, ref _currenttrack))
                {
                    if (_currenttrack != null) OnTrackChanged();
                }
            }
        }

        public long TrackLength
        {
            get { return SoundSource == null ? 0 : SoundSource.Length; }
        }

        public bool IsPlaying
        {
            get
            {
                return (_soundOut != null && (!_isfadingout && _soundOut.PlaybackState == PlaybackState.Playing));
            }
        }

        public PlaybackState CurrentState
        {
            get
            {
                if (_soundOut == null) { return PlaybackState.Stopped; } else { return _soundOut.PlaybackState; }
            }
        }

        public TimeSpan CurrentTrackPosition
        {
            get
            {
                try
                {
                    return SoundSource != null ? SoundSource.GetPosition() : TimeSpan.Zero;
                }
                catch (Exception)
                {
                    return TimeSpan.Zero; //Sometimes it crashes
                }
            }
        }

        public TimeSpan CurrentTrackLength
        {
            get
            {
                try
                {
                    return SoundSource != null ? SoundSource.GetLength() : TimeSpan.Zero;
                }
                catch (Exception)
                {
                    return TimeSpan.Zero;
                }
            }
        }

        public IWaveSource SoundSource { get; protected set; }

        public ConfigSettings Settings { get { return HurricaneSettings.Instance.Config; } }

        public SoundOutManager SoundOutManager { get; set; }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                SetProperty(value, ref _isLoading);
            }
        }
        
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                SetProperty(value, ref _isEnabled);
            }
        }
        #endregion

        #region Members
        private readonly Crossfade _crossfade;
        private readonly VolumeFading _fader;

        private ISoundOut _soundOut;
        private SimpleNotificationSource _simpleNotificationSource;
        private SingleBlockNotificationStream _singleBlockNotificationStream;

        private bool _manualstop;
        private bool _isfadingout;
        private bool _playAfterLoading;
        
        #endregion

        #region Equalizer
        public Equalizer MusicEqualizer { get; set; }

        private EqualizerSettings _equalizerSettings;
        public EqualizerSettings EqualizerSettings
        {
            get { return _equalizerSettings; }
            set
            {
                if (SetProperty(value, ref _equalizerSettings)) value.EqualizerChanged += value_EqualizerChanged;
            }
        }

        void value_EqualizerChanged(object sender, EqualizerChangedEventArgs e)
        {
            SetEqualizerValue(e.EqualizerValue, e.EqualizerNumber);
        }

        protected void SetEqualizerValue(double value, int number)
        {
            if (MusicEqualizer == null) return;
            var perc = (value / 100);
            var newvalue = (float)(perc * MaxDB);
            //the tag of the trackbar contains the index of the filter
            EqualizerFilter filter = MusicEqualizer.SampleFilters[number];
            filter.AverageGainDB = newvalue;
        }

        protected void SetAllEqualizerSettings()
        {
            for (int i = 0; i < EqualizerSettings.Bands.Count; i++)
            {
                SetEqualizerValue(EqualizerSettings.Bands[i].Value, i);
            }
        }

        #endregion

        #region Public Methods
        public async Task<bool> OpenTrack(PlayableBase track)
        {
            if (!IsEnabled)
            {
                OnSoundOutErrorOccurred(Application.Current.Resources["NoSoundOutDeviceFound"].ToString());
                return false;
            }

            _playAfterLoading = false;
            IsLoading = true;
            StopPlayback();
            if (CurrentTrack != null) { CurrentTrack.IsOpened = false; CurrentTrack.Unload(); }
            if (SoundSource != null && !_crossfade.IsCrossfading) { SoundSource.Dispose(); }
            track.IsOpened = true;
            CurrentTrack = track;
            var t = Task.Run(() => track.Load());
            Equalizer equalizer;

            var result = await SetSoundSource(track);
            switch (result.State)
            {
                case State.False:
                    track.IsOpened = false;
                    return false;
                case State.Exception:
                    track.IsOpened = false;
                    IsLoading = false;
                    if (ExceptionOccurred != null) ExceptionOccurred(this, (Exception)result.CustomState);
                    return false;
            }

            if (Settings.SampleRate == -1 && SoundSource.WaveFormat.SampleRate < 44100)
            {
                SoundSource = SoundSource.ChangeSampleRate(44100);
            }
            else if (Settings.SampleRate > -1) { SoundSource = SoundSource.ChangeSampleRate(Settings.SampleRate); }
            SoundSource = SoundSource
                .AppendSource(Equalizer.Create10BandEqualizer, out equalizer)
                .AppendSource(x => new SingleBlockNotificationStream(x), out _singleBlockNotificationStream)
                .AppendSource(x => new SimpleNotificationSource(x) { Interval = 100 }, out _simpleNotificationSource)
                .ToWaveSource(Settings.WaveSourceBits);

            MusicEqualizer = equalizer;
            SetAllEqualizerSettings();
            _simpleNotificationSource.BlockRead += notifysource_BlockRead;
            _singleBlockNotificationStream.SingleBlockRead += notificationSource_SingleBlockRead;

            _analyser = new SampleAnalyser(FFTSize);
            _analyser.Initialize(SoundSource.WaveFormat);

            try
            {
                _soundOut.Initialize(SoundSource);
            }
            catch (Exception ex)
            {
                track.IsOpened = false;
                IsLoading = false;
                OnSoundOutErrorOccurred(ex.Message);
                return false;
            }

            OnPropertyChanged("TrackLength");
            OnPropertyChanged("CurrentTrackLength");

            CurrentStateChanged();
            _soundOut.Volume = Volume;
            if (StartVisualization != null) StartVisualization(this, EventArgs.Empty);
            track.LastTimePlayed = DateTime.Now;
            if (_crossfade.IsCrossfading)
                _fader.CrossfadeIn(_soundOut, Volume);
            IsLoading = false;
            if (_playAfterLoading) TogglePlayPause();
            await t;
            return true;
        }

        private CancellationTokenSource _cts;
        private async Task<Result> SetSoundSource(PlayableBase track)
        {
            if (_cts != null)
                _cts.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            IWaveSource result;

            try
            {
                result = await track.GetSoundSource();
                if (token.IsCancellationRequested)
                {
                    result.Dispose();
                    return new Result(State.False);
                }
            }
            catch (Exception ex)
            {
                return new Result(State.Exception, ex);
            }

            SoundSource = result;
            return new Result(State.True);
        }

        public void StopPlayback()
        {
            if (_soundOut.PlaybackState == PlaybackState.Playing || _soundOut.PlaybackState == PlaybackState.Paused)
            {
                _manualstop = true;
                _soundOut.Stop();
                CurrentStateChanged();
            }
        }

        public void KickTrack()
        {
            CurrentTrack.Unload();
            CurrentTrack = null;
            SoundSource = null;
            OnPropertyChanged("TrackLength");
            OnPropertyChanged("CurrentTrackLength");
            OnPropertyChanged("Position");
            OnPropertyChanged("CurrentTrackPosition");
            CurrentStateChanged();
        }

        public async void TogglePlayPause()
        {
            if (IsLoading)
            {
                _playAfterLoading = !_playAfterLoading;
                return;
            }

            if (CurrentTrack == null) return;
            if (_fader != null && _fader.IsFading) { _fader.CancelFading(); _fader.WaitForCancel(); }
            if (_soundOut.PlaybackState == PlaybackState.Playing)
            {
                if (_crossfade != null && _crossfade.IsCrossfading) { _crossfade.CancelFading(); }
                _isfadingout = true;
                await _fader.FadeOut(_soundOut, this.Volume);
                _soundOut.Pause();
                CurrentStateChanged();
                _isfadingout = false;
            }
            else
            {
                _soundOut.Play();
                CurrentStateChanged();
                await _fader.FadeIn(_soundOut, this.Volume);
            }
        }
        #endregion

        #region Protected Methods
        protected void CurrentStateChanged()
        {
            OnPropertyChanged("IsPlaying");
            OnPropertyChanged("CurrentState");
            if (PlayStateChanged != null) PlayStateChanged(this, EventArgs.Empty);
            if (PlaybackStateChanged != null) PlaybackStateChanged(this, new PlayStateChangedEventArgs(this.CurrentState));
        }

        void notificationSource_SingleBlockRead(object sender, SingleBlockReadEventArgs e)
        {
            if (_analyser != null)
                _analyser.Add(e.Left, e.Right);
        }

        void notifysource_BlockRead(object sender, EventArgs e)
        {
            OnPropertyChanged("Position");
            OnPropertyChanged("CurrentTrackPosition");
            var seconds = (int)CurrentTrackPosition.TotalSeconds;
            var totalseconds = (int)CurrentTrackLength.TotalSeconds;
            if (PositionChanged != null)
                Application.Current.Dispatcher.Invoke(() => PositionChanged(this, new PositionChangedEventArgs(seconds, totalseconds)));

            if (Settings.IsCrossfadeEnabled && totalseconds - Settings.CrossfadeDuration > 6 && !_crossfade.IsCrossfading && totalseconds - seconds < Settings.CrossfadeDuration)
            {
                _fader.OutDuration = totalseconds - seconds;
                _crossfade.FadeOut(Settings.CrossfadeDuration, _soundOut);
                _simpleNotificationSource.BlockRead -= notifysource_BlockRead;
                _singleBlockNotificationStream.SingleBlockRead -= notificationSource_SingleBlockRead;
                _soundOut.Stopped -= soundOut_Stopped;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RefreshSoundOut();
                    OnTrackFinished();
                });
            }
        }

        #endregion

        #region Constructor
        public CSCoreEngine()
        {
            _fader = new VolumeFading();
            _crossfade = new Crossfade();
            SoundOutManager = new SoundOutManager(this);
            SoundOutManager.RefreshSoundOut += (sender, args) => Refresh();
            SoundOutManager.Enable += (sender, args) => IsEnabled = true;
            SoundOutManager.Disable += (sender, args) => IsEnabled = false;
            SoundOutManager.Activate();
            if (IsEnabled) RefreshSoundOut();
        }

        #endregion

        #region SoundOut

        public void RefreshSoundOut()
        {
            _soundOut = SoundOutManager.GetNewSoundSource();
            _soundOut.Stopped += soundOut_Stopped;
        }

        public async void Refresh()
        {
            var position = Position;
            var isplaying = IsPlaying;
            if (_soundOut != null) { StopPlayback(); _soundOut.Dispose(); }
            if (SoundSource != null) SoundSource.Dispose();
            RefreshSoundOut();
            if (CurrentTrack != null)
            {
                if (await OpenTrack(CurrentTrack))
                {
                    Position = position;
                    if (isplaying) TogglePlayPause();
                }
            }
        }

        void soundOut_Stopped(object sender, PlaybackStoppedEventArgs e)
        {
            if (_isdisposing) return;
            if (_manualstop) { _manualstop = false; return; }
            if (!e.HasError) OnTrackFinished();
            CurrentStateChanged();
        }
        #endregion

        #region Visualization Support
        SampleAnalyser _analyser;
        public bool GetFFTData(float[] fftDataBuffer)
        {
            _analyser.CalculateFFT(fftDataBuffer);
            return IsPlaying;
        }

        public int GetFFTFrequencyIndex(int frequency)
        {
            double f;
            if (SoundSource != null)
            {
                f = SoundSource.WaveFormat.SampleRate / 2.0;
            }
            else
            {
                f = 22050; //44100 / 2
            }
            return Convert.ToInt32((frequency / f) * (FFTSize / 2));
        }
        #endregion

        #region IDisposable Support

        private bool _isdisposing;
        public void Dispose()
        {
            _isdisposing = true;
            _fader.Dispose();
             SoundOutManager.Dispose();

            if (_soundOut != null)
            {
                if (_fader.IsFading) { _fader.CancelFading(); _fader.WaitForCancel(); }
                _soundOut.Dispose();
                _crossfade.CancelFading();
            }
            if (SoundSource != null) SoundSource.Dispose();
        }

        #endregion
    }
}