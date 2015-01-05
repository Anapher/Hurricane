using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using CSCore.Streams;
using Hurricane.Music.Data;
using Hurricane.Music.MusicDatabase.EventArgs;
using Hurricane.Music.Visualization;
using Hurricane.Settings;
using Hurricane.ViewModelBase;
using WPFSoundVisualizationLib;
using Equalizer = CSCore.Streams.Equalizer;

namespace Hurricane.Music
{
    public class CSCoreEngine : PropertyChangedBase, IDisposable, ISpectrumPlayer
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
            get { return SoundSource == null ? 0 : SoundSource.Position; }
            set
            {
                if (SoundSource != null)
                    SoundSource.Position = value;
                OnPropertyChanged("CurrentTrackPosition");
                if (PositionChanged != null) PositionChanged(this, new PositionChangedEventArgs((int)this.CurrentTrackPosition.TotalSeconds, (int)this.CurrentTrackLength.TotalSeconds));
            }
        }

        private Track _currenttrack;
        public Track CurrentTrack
        {
            get { return _currenttrack; }
            protected set
            {
                if (SetProperty(value, ref _currenttrack))
                {
                    if (TrackChanged != null && _currenttrack != null) TrackChanged(this, new TrackChangedEventArgs(value));
                }
            }
        }

        public long TrackLength
        {
            get { return SoundSource == null ? 0 : SoundSource.Length; }
        }

        public bool IsPlaying
        {
            get { return (_soundOut != null && (!_isfadingout && _soundOut.PlaybackState == PlaybackState.Playing)); }
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
        #endregion

        #region Members
        private string _currentDeviceId;
        protected WasapiOut _soundOut;
        protected MMNotificationClient _client;
        protected bool _manualstop;
        protected VolumeFading _fader;
        protected bool _isfadingout;

        #endregion

        #region Equalizer
        public Equalizer MusicEqualizer { get; set; }

        protected EqualizerSettings _equalizersettings;
        public EqualizerSettings EqualizerSettings { get { return _equalizersettings; } set { _equalizersettings = value; value.EqualizerChanged += value_EqualizerChanged; } }

        void value_EqualizerChanged(object sender, EqualizerChangedEventArgs e)
        {
            SetEqualizerValue(e.EqualizerValue, e.EqualizerNumber);
        }

        protected void SetEqualizerValue(double value, int number)
        {
            if (MusicEqualizer == null) return;
            double perc = (value / (double)100);
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
        public void OpenTrack(Track track)
        {
            if (CurrentTrack != null) { CurrentTrack.IsPlaying = false; CurrentTrack.Unload(); }
            if (SoundSource != null) { SoundSource.Dispose(); }
            track.IsPlaying = true;
            SoundSource = CodecFactory.Instance.GetCodec(track.Path);

            if (Settings.SampleRate == -1 && SoundSource.WaveFormat.SampleRate < 44100)
            {
                SoundSource = SoundSource.ChangeSampleRate(44100);
            }
            else if (Settings.SampleRate > -1) { SoundSource.ChangeSampleRate(Settings.SampleRate); }

            Equalizer equalizer;
            SimpleNotificationSource simpleNotificationSource;
            SingleBlockNotificationStream singleBlockNotificationStream;

            SoundSource = SoundSource
    .AppendSource(Equalizer.Create10BandEqualizer, out equalizer)
    .AppendSource(x => new SimpleNotificationSource(x) { Interval = 100 }, out simpleNotificationSource)
    .AppendSource(x => new SingleBlockNotificationStream(x), out singleBlockNotificationStream)
    .ToWaveSource(Settings.WaveSourceBits);

            MusicEqualizer = equalizer;
            SetAllEqualizerSettings();
            simpleNotificationSource.BlockRead += notifysource_BlockRead;
            singleBlockNotificationStream.SingleBlockRead += notificationSource_SingleBlockRead;

            _analyser = new SampleAnalyser(FFTSize);
            _analyser.Initialize(SoundSource.WaveFormat);
            _soundOut.Initialize(SoundSource);

            CurrentTrack = track;
            OnPropertyChanged("TrackLength");
            OnPropertyChanged("CurrentTrackLength");
            CurrentStateChanged();
            _soundOut.Volume = Volume;

            if (StartVisualization != null) StartVisualization(this, EventArgs.Empty);
            track.LastTimePlayed = DateTime.Now;
            track.Load();
        }

        public void StopPlayback()
        {
            if (_soundOut.PlaybackState == PlaybackState.Playing || _soundOut.PlaybackState == PlaybackState.Paused)
            {
                _manualstop = true;
                _soundOut.Stop();
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
            if (CurrentTrack == null) return;
            if (_fader != null && _fader.IsFading) { _fader.CancelFading(); _fader.WaitForCancel(); }
            if (_soundOut.PlaybackState == PlaybackState.Playing)
            {
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
            if (PositionChanged != null) Application.Current.Dispatcher.Invoke(() => PositionChanged(this, new PositionChangedEventArgs((int)this.CurrentTrackPosition.TotalSeconds, (int)this.CurrentTrackLength.TotalSeconds)));
        }
        #endregion

        #region Constructor
        public CSCoreEngine()
        {
            _client = new MMNotificationClient();
            RefreshSoundOut();
            _client.DefaultDeviceChanged += client_DefaultDeviceChanged;
            _fader = new VolumeFading();
        }

        #endregion

        #region SoundOut
        void client_DefaultDeviceChanged(object sender, DefaultDeviceChangedEventArgs e)
        {
            if (Settings.SoundOutDeviceID == "-0" && e.DeviceID != _currentDeviceId)
            {
                _currentDeviceId = e.DeviceID;
                Thread t = new Thread(() =>
                {
                    Thread.Sleep(100);
                    Application.Current.Dispatcher.Invoke(UpdateSoundOut);
                }) { IsBackground = true };
                t.Start();
            }
        }

        public void RefreshSoundOut()
        {
            MMDevice device = null;
            using (MMDeviceEnumerator enumerator = new MMDeviceEnumerator())
            {
                if (Settings.SoundOutDeviceID == "-0")
                {
                    device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                }
                else
                {
                    using (MMDeviceCollection devices = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                    {
                        foreach (MMDevice currentDevice in devices)
                        {
                            if (currentDevice.DeviceID == Settings.SoundOutDeviceID)
                            {
                                device = currentDevice;
                            }
                        }
                        if (device == null)
                        {
                            Settings.SoundOutDeviceID = "-0";
                            RefreshSoundOut();
                            return;
                        }
                    }
                }
            }
            _soundOut = new WasapiOut {Device = device, Latency = 100};
            _soundOut.Stopped += soundOut_Stopped;
        }

        public void UpdateSoundOut()
        {
            long position = Position;
            bool isplaying = IsPlaying;
            StopPlayback();
            if (_soundOut != null) _soundOut.Dispose();
            RefreshSoundOut();
            Track currenttrack = CurrentTrack;
            CurrentTrack = null;
            if (currenttrack != null) OpenTrack(currenttrack);
            Position = position;
            if (isplaying) TogglePlayPause();
        }

        void soundOut_Stopped(object sender, PlaybackStoppedEventArgs e)
        {
            if (_isdisposing) return;
            if (_manualstop) { _manualstop = false; return; }
            if (TrackFinished != null) TrackFinished(this, EventArgs.Empty);
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
        protected bool _isdisposing;
        public void Dispose()
        {
            _isdisposing = true;
            if (_soundOut != null)
            {
                if (_fader.IsFading) { _fader.CancelFading(); _fader.WaitForCancel(); }
                _soundOut.Dispose();
                _fader.Dispose();
            }
            if (SoundSource != null) SoundSource.Dispose();
            if (_client != null) _client.Dispose();
        }
        #endregion

        #region Static Methods
        public static List<AudioDevice> GetAudioDevices()
        {
            List<AudioDevice> result = new List<AudioDevice>();
            using (MMDeviceEnumerator enumerator = new MMDeviceEnumerator())
            {
                MMDevice standarddevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                using (MMDeviceCollection devices = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                {
                    result.Add(new AudioDevice() { ID = "-0", Name = "Windows Default" });
                    result.AddRange(devices.Select(device => new AudioDevice() {ID = device.DeviceID, Name = device.FriendlyName, IsDefault = standarddevice.DeviceID == device.DeviceID}));
                }
            }

            return result;
        }
        #endregion
    }
}