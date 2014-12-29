using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurricane.ViewModelBase;
using CSCore;
using CSCore.SoundOut;
using CSCore.Codecs;
using System.IO;
using CSCore.CoreAudioAPI;
using CSCore.Streams;

namespace Hurricane.Music
{
    public class CSCoreEngine : PropertyChangedBase, IDisposable, WPFSoundVisualizationLib.ISpectrumPlayer
    {
        #region Consts
        const int FFTSize = 4096;

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
        private float volume = 1.0f;
        public float Volume
        {
            get { return volume; }
            set
            {
                if (SetProperty(value, ref volume))
                {
                    if (soundOut != null && soundOut.WaveSource != null)
                        soundOut.Volume = value;
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

        private Track currenttrack;
        public Track CurrentTrack
        {
            get { return currenttrack; }
            protected set
            {
                if (SetProperty(value, ref currenttrack))
                {
                    if (TrackChanged != null && currenttrack != null) TrackChanged(this, new TrackChangedEventArgs(value));
                }
            }
        }

        public long TrackLength
        {
            get { return SoundSource == null ? 0 : SoundSource.Length; }
        }

        public bool IsPlaying
        {
            get { return (soundOut == null ? false : (isfadingout ? false : soundOut.PlaybackState == PlaybackState.Playing)); }
        }

        public PlaybackState CurrentState
        {
            get
            {
                if (soundOut == null) { return PlaybackState.Stopped; } else { return soundOut.PlaybackState; }
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

        public Settings.ConfigSettings Settings { get { return Hurricane.Settings.HurricaneSettings.Instance.Config; } }
        #endregion

        #region Members
        private string CurrentDeviceID;
        protected WasapiOut soundOut;
        protected MMNotificationClient client;
        protected bool manualstop = false;
        protected VolumeFading fader;
        protected bool isfadingout = false;

        #endregion

        #region Equalizer
        public Equalizer MusicEqualizer { get; set; }
        protected EqualizerSettings equalizersettings;
        public EqualizerSettings EqualizerSettings { get { return equalizersettings; } set { equalizersettings = value; value.EqualizerChanged += value_EqualizerChanged; } }

        void value_EqualizerChanged(object sender, EqualizerChangedEventArgs e)
        {
            SetEqualizerValue(e.EqualizerValue, e.EqualizerNumber);
        }

        private const double MaxDB = 20;

        protected void SetEqualizerValue(double value, int number)
        {
            if (MusicEqualizer == null) return;
            double perc = (value / (double)100);
            var newvalue = (float)(perc * MaxDB);

            //the tag of the trackbar contains the index of the filter
            EqualizerFilter filter = MusicEqualizer.SampleFilters[number];
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

            this.MusicEqualizer = equalizer;
            SetAllEqualizerSettings();
            simpleNotificationSource.BlockRead += notifysource_BlockRead;
            singleBlockNotificationStream.SingleBlockRead += notificationSource_SingleBlockRead;

            analyser = new Visualization.SampleAnalyser(FFTSize);
            analyser.Initialize(SoundSource.WaveFormat);
            soundOut.Initialize(SoundSource);

            CurrentTrack = track;
            OnPropertyChanged("TrackLength");
            OnPropertyChanged("CurrentTrackLength");
            CurrentStateChanged();
            soundOut.Volume = Volume;

            if (StartVisualization != null) StartVisualization(this, EventArgs.Empty);
            track.LastTimePlayed = DateTime.Now;
            track.Load();
        }

        public void StopPlayback()
        {
            if (soundOut.PlaybackState == PlaybackState.Playing || soundOut.PlaybackState == PlaybackState.Paused)
            {
                manualstop = true;
                soundOut.Stop();
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
            if (fader != null && fader.IsFading) { fader.CancelFading(); fader.WaitForCancel(); }
            if (soundOut.PlaybackState == PlaybackState.Playing)
            {
                isfadingout = true;
                await fader.FadeOut(soundOut, this.Volume);
                soundOut.Pause();
                CurrentStateChanged();
                isfadingout = false;
            }
            else
            {
                soundOut.Play();
                CurrentStateChanged();
                await fader.FadeIn(soundOut, this.Volume);
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
            if (analyser != null)
                analyser.Add(e.Left, e.Right);
        }

        void notifysource_BlockRead(object sender, EventArgs e)
        {
            OnPropertyChanged("Position");
            OnPropertyChanged("CurrentTrackPosition");
            if (PositionChanged != null) System.Windows.Application.Current.Dispatcher.Invoke(() => PositionChanged(this, new PositionChangedEventArgs((int)this.CurrentTrackPosition.TotalSeconds, (int)this.CurrentTrackLength.TotalSeconds)));
        }
        #endregion

        #region Constructor
        public CSCoreEngine()
        {
            client = new MMNotificationClient();
            RefreshSoundOut();
            client.DefaultDeviceChanged += client_DefaultDeviceChanged;
            fader = new VolumeFading();
        }

        #endregion

        #region SoundOut
        void client_DefaultDeviceChanged(object sender, DefaultDeviceChangedEventArgs e)
        {
            if (Settings.SoundOutDeviceID == "-0" && e.DeviceID != CurrentDeviceID)
            {
                CurrentDeviceID = e.DeviceID;
                System.Threading.Thread t = new System.Threading.Thread(() => { System.Threading.Thread.Sleep(100); System.Windows.Application.Current.Dispatcher.Invoke(() => UpdateSoundOut()); });
                t.IsBackground = true;
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
                    using (MMDeviceCollection Devices = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                    {
                        foreach (MMDevice CurrentDevice in Devices)
                        {
                            if (CurrentDevice.DeviceID == Settings.SoundOutDeviceID)
                            {
                                device = CurrentDevice;
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
            soundOut = new WasapiOut();
            soundOut.Device = device;
            soundOut.Latency = 100;
            soundOut.Stopped += soundOut_Stopped;
        }

        public void UpdateSoundOut()
        {
            long position = this.Position;
            bool isplaying = this.IsPlaying;
            StopPlayback();
            if (soundOut != null) soundOut.Dispose();
            RefreshSoundOut();
            Track currenttrack = CurrentTrack;
            CurrentTrack = null;
            if (currenttrack != null) OpenTrack(currenttrack);
            this.Position = position;
            if (isplaying) TogglePlayPause();
        }

        void soundOut_Stopped(object sender, PlaybackStoppedEventArgs e)
        {
            if (isdisposing) return;
            if (manualstop) { manualstop = false; return; }
            TrackFinished(this, EventArgs.Empty);
            CurrentStateChanged();
        }
        #endregion

        #region Visualization Support
        Visualization.SampleAnalyser analyser;
        public bool GetFFTData(float[] fftDataBuffer)
        {
            analyser.CalculateFFT(fftDataBuffer);
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
        protected bool isdisposing;
        public void Dispose()
        {
            isdisposing = true;
            if (soundOut != null)
            {
                if (fader.IsFading) { fader.CancelFading(); fader.WaitForCancel(); }
                soundOut.Dispose();
                fader.Dispose();
            }
            if (SoundSource != null) SoundSource.Dispose();
            if (client != null) client.Dispose();
        }
        #endregion

        #region Static Methods
        public static List<AudioDevice> GetAudioDevices()
        {
            List<AudioDevice> result = new List<AudioDevice>();
            using (MMDeviceEnumerator enumerator = new MMDeviceEnumerator())
            {
                MMDevice standarddevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                using (MMDeviceCollection Devices = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                {
                    result.Add(new AudioDevice() { ID = "-0", Name = "Windows Default" });
                    foreach (MMDevice device in Devices)
                    {
                        result.Add(new AudioDevice() { ID = device.DeviceID, Name = device.FriendlyName, IsDefault = standarddevice.DeviceID == device.DeviceID });
                    }
                }
            }

            return result;
        }
        #endregion
    }
}