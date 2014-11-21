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
        #region Commands
        private RelayCommand toggleplaypausecommand;
        public RelayCommand TogglePlayPauseCommand
        {
            get
            {
                if (toggleplaypausecommand == null)
                    toggleplaypausecommand = new RelayCommand((object parameter) => { TogglePlayPause(); });
                return toggleplaypausecommand;
            }
        }
        #endregion

        #region consts

        const int FFTSize = 4096;

        #endregion

        #region Events
        public event EventHandler StartVisualization;
        public event EventHandler TrackFinished;
        public event EventHandler<TrackChangedEventArgs> TrackChanged;
        public event EventHandler PlayStateChanged;
        #endregion

        private float volume = 1.0f;
        public float Volume
        {
            get { return volume; }
            set
            {
                SetProperty(value, ref volume);
                if (soundOut != null)
                    soundOut.Volume = value;
            }
        }

        public long Position
        {
            get { return SoundSource == null ? 0 : SoundSource.Position; }
            set
            {
                if (SoundSource != null)
                    SoundSource.Position = value;
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
            get { return (soundOut == null ? false : soundOut.PlaybackState == PlaybackState.Playing); }
        }

        public PlaybackState CurrentState
        {
            get
            {
                if (soundOut == null) { return PlaybackState.Stopped; } else { return soundOut.PlaybackState; }
            }
        }

        protected WasapiOut soundOut;

        public IWaveSource SoundSource { get; protected set; }

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
            EqFilterEntry filter = MusicEqualizer.SampleFilters[number];
            filter.SetGain(newvalue);
        }

        protected void SetAllEqualizerSettings()
        {
            for (int i = 0; i < EqualizerSettings.Bands.Count; i++)
            {
                SetEqualizerValue(EqualizerSettings.Bands[i].Value, i);
            }
        }

        #endregion

        public void OpenTrack(Track track)
        {
            if (CurrentTrack != null) { CurrentTrack.IsPlaying = false; CurrentTrack.Unload(); }
            if (SoundSource != null) SoundSource.Dispose();
            track.IsPlaying = true;
            SoundSource = CodecFactory.Instance.GetCodec(track.Path);
            if (Settings.SampleRate == -1 && SoundSource.WaveFormat.SampleRate < 44100)
            {
                SoundSource = SoundSource.ChangeSampleRate(44100);
            }
            else if (Settings.SampleRate > -1) { SoundSource.ChangeSampleRate(Settings.SampleRate); }

            SimpleNotificationSource notifysource = new SimpleNotificationSource(SoundSource);
            notifysource.Interval = 100;
            notifysource.BlockRead += notifysource_BlockRead;
            this.MusicEqualizer = Equalizer.Create10BandEqualizer(notifysource);
            SetAllEqualizerSettings();

            SingleBlockNotificationStream notificationSource = new SingleBlockNotificationStream(MusicEqualizer);
            notificationSource.SingleBlockRead += notificationSource_SingleBlockRead;

            analyser = new Visualization.SampleAnalyser(FFTSize);
            analyser.Initialize(notificationSource.WaveFormat);
            soundOut.Initialize(notificationSource.ToWaveSource(Settings.WaveSourceBits));
            track.Load();
            CurrentTrack = track;
            OnPropertyChanged("TrackLength");
            CurrentStateChanged();
            soundOut.Volume = Volume;
            if (StartVisualization != null) StartVisualization(this, EventArgs.Empty);
            track.LastTimePlayed = DateTime.Now;
        }

        protected void CurrentStateChanged()
        {
            OnPropertyChanged("IsPlaying");
            OnPropertyChanged("CurrentState");
            if (PlayStateChanged != null) PlayStateChanged(this, EventArgs.Empty);
        }

        void soundOut_Stopped(object sender, PlaybackStoppedEventArgs e)
        {
            if (manualstop) { manualstop = false; return; }
            System.Diagnostics.Debug.Print("end of track");
            TrackFinished(this, EventArgs.Empty);
            CurrentStateChanged();
        }

        private bool manualstop = false;
        public void StopPlayback()
        {
            if (soundOut.PlaybackState == PlaybackState.Playing || soundOut.PlaybackState == PlaybackState.Paused)
            {
                manualstop = true;
                soundOut.Stop();
            }
        }

        void notificationSource_SingleBlockRead(object sender, SingleBlockReadEventArgs e)
        {
            if (analyser != null)
                analyser.Add(e.Left, e.Right);
        }

        void notifysource_BlockRead(object sender, EventArgs e)
        {
            OnPropertyChanged("Position");
        }

        public void TogglePlayPause()
        {
            if (CurrentTrack == null) return;
            if (soundOut.PlaybackState == PlaybackState.Playing)
            {
                soundOut.Pause();
            }
            else
            {
                soundOut.Play();
            }
            CurrentStateChanged();
        }

        MMNotificationClient client = new MMNotificationClient();
        public CSCoreEngine()
        {
            RefreshSoundOut();
            client.DefaultDeviceChanged += client_DefaultDeviceChanged;
        }

        private string CurrentDeviceID;
        void client_DefaultDeviceChanged(object sender, DefaultDeviceChangedEventArgs e)
        {
            if (Settings.SoundOutDeviceID == "-0" && e.DeviceID != CurrentDeviceID)
            {
                CurrentDeviceID = e.DeviceID;
                System.Diagnostics.Debug.Print("SoundOutDevice changed");
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
            soundOut.Volume = Volume;
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

        public Settings.ConfigSettings Settings { get { return Hurricane.Settings.HurricaneSettings.Instance.Config; } }

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
        public void Dispose()
        {
            if (soundOut != null) { StopPlayback(); soundOut.Dispose(); }
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

        public class AudioDevice
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public bool IsDefault { get; set; }

            public override string ToString()
            {
                return this.IsDefault ? Name + " (Default)" : Name;
            }
        }
        #endregion
    }
}