using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using CSCore.SoundOut.DirectSound;
using Hurricane.Music.Data;
using Hurricane.Settings;
using Microsoft.Win32;

namespace Hurricane.Music.AudioEngine
{
    public class SoundOutManager : IDisposable
    {
        #region consts
        public const string DefaultDevicePlaceholder = "-0";

        #endregion

        #region events
        public event EventHandler RefreshSoundOut;
        public event EventHandler Disable;
        public event EventHandler Enable;

        #endregion

        #region public properties
        public CSCoreEngine BaseEngine { get; set; }
        public ObservableCollection<SoundOutRepresenter> SoundOutList { get; set; }

        #endregion

        private readonly MMNotificationClient _mmNotificationClient;
        private string _currentDeviceId;

        public SoundOutManager(CSCoreEngine engine)
        {
            BaseEngine = engine;
            _mmNotificationClient = new MMNotificationClient();
        }

        public void Activate()
        {
            //That we can subscripe to the events
            _mmNotificationClient.DefaultDeviceChanged += _mmNotificationClient_DefaultDeviceChanged;
            _mmNotificationClient.DeviceRemoved += _mmNotificationClient_DeviceRemoved;
            _mmNotificationClient.DeviceStateChanged += _mmNotificationClient_DeviceStateChanged;
            _mmNotificationClient.DeviceAdded += _mmNotificationClient_DeviceAdded;

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            RefreshSoundOutRepresenter();
            CheckCurrentState();
        }

        protected void OnRefreshSoundOut()
        {
            if (RefreshSoundOut != null) RefreshSoundOut(this, EventArgs.Empty);
        }

        public ISoundOut GetNewSoundSource()
        {
            var settings = HurricaneSettings.Instance.Config;

            MMDevice defaultDevice;
            using (var enumerator = new MMDeviceEnumerator())
            {
                try
                {
                    defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                }
                catch (CoreAudioAPIException)
                {
                    defaultDevice = null;
                }
            }


            if (settings.SoundOutMode == SoundOutMode.DirectSound)
            {
                var enumerator = new DirectSoundDeviceEnumerator();
                if (enumerator.Devices.Count == 0)
                {
                    settings.SoundOutMode = SoundOutMode.WASAPI;
                    return GetNewSoundSource();
                }

                DirectSoundDevice device;
                if (settings.SoundOutDeviceID == DefaultDevicePlaceholder)
                {
                    device = defaultDevice != null
                        ? enumerator.Devices.FirstOrDefault(x => x.Description == defaultDevice.FriendlyName) ??
                          enumerator.Devices.First()
                        : enumerator.Devices.First();
                }
                else
                {
                    device = enumerator.Devices.FirstOrDefault(x => x.Guid.ToString() == settings.SoundOutDeviceID);
                    if (device == null)
                    {
                        settings.SoundOutDeviceID = DefaultDevicePlaceholder;
                        return GetNewSoundSource();
                    }
                }

                _currentDeviceId = device.Guid.ToString();
                return new DirectSoundOut { Device = device.Guid, Latency = settings.Latency };
            }
            else
            {
                using (var enumerator = new MMDeviceEnumerator())
                {
                    using (var devices = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                    {
                        if (defaultDevice == null || devices.Count == 0)
                        {
                            settings.SoundOutMode = SoundOutMode.DirectSound;
                            return GetNewSoundSource();
                        }

                        MMDevice device;
                        if (settings.SoundOutDeviceID == DefaultDevicePlaceholder)
                        {
                            device = defaultDevice;
                        }
                        else
                        {
                            device = devices.FirstOrDefault(x => x.DeviceID == settings.SoundOutDeviceID);

                            if (device == null)
                            {
                                settings.SoundOutDeviceID = DefaultDevicePlaceholder;
                                return GetNewSoundSource();
                            }
                        }
                        _currentDeviceId = device.DeviceID;
                        return new WasapiOut { Device = device, Latency = settings.Latency };
                    }
                }
            }
        }

        #region representer
        void RefreshSoundOutRepresenter()
        {
            var result = new ObservableCollection<SoundOutRepresenter>();
            using (var enumerator = new MMDeviceEnumerator())
            {
                MMDevice standarddevice;
                try
                {
                    standarddevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                }
                catch (CoreAudioAPIException)
                {
                    standarddevice = null;
                }
                
                if (WasapiOut.IsSupportedOnCurrentPlatform)
                {
                    var wasApiItem = new SoundOutRepresenter(deviceId =>
                    {
                        using (var mmdeviceEnumerator = new MMDeviceEnumerator())
                        {
                            var device =
                                mmdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active)
                                    .FirstOrDefault(x => x.DeviceID == deviceId);
                            return device == null ? null : new AudioDevice(device.DeviceID, device.FriendlyName);
                        }
                    }) { Name = "WASAPI", SoundOutMode = SoundOutMode.WASAPI };

                    using (var devices = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                    {
                        foreach (var device in devices.Select(device => new AudioDevice(device.DeviceID, device.FriendlyName, standarddevice != null && standarddevice.DeviceID == device.DeviceID)))
                        {
                            wasApiItem.AudioDevices.Add(device);
                        }
                    }

                    CheckDefaultAudioDevice(wasApiItem);

                    result.Add(wasApiItem);
                }

                var directSoundItem = new SoundOutRepresenter(deviceId =>
                {
                    var device =
                        new DirectSoundDeviceEnumerator().Devices
                            .FirstOrDefault(x => x.Guid.ToString() == deviceId);
                    return device == null ? null : new AudioDevice(device.Guid.ToString(), device.Description);
                }) { Name = "DirectSound", SoundOutMode = SoundOutMode.DirectSound };

                foreach (var device in new DirectSoundDeviceEnumerator().Devices.Select(x => new AudioDevice(x.Guid.ToString(), x.Description, standarddevice != null && x.Description == standarddevice.FriendlyName)))
                {
                    directSoundItem.AudioDevices.Add(device);
                }

                CheckDefaultAudioDevice(directSoundItem);

                result.Add(directSoundItem);
            }

            SoundOutList = result;
        }

        void CheckDefaultAudioDevice(SoundOutRepresenter representer)
        {
            if (representer.AudioDevices.All(x => x.ID == DefaultDevicePlaceholder))
            {
                representer.AudioDevices.Clear(); //No default item if there are no other devices
            }
            else if(representer.AudioDevices.Count > 0 && representer.AudioDevices.All(x => x.ID != DefaultDevicePlaceholder))
            {
                representer.AudioDevices.Insert(0, new AudioDevice("-0", "Windows Default"));
            }
        }

        void UpdateDefaultAudioDevice()
        {
            MMDevice defaultAudioEndpoint;
            using (var enumerator = new MMDeviceEnumerator())
            {
                defaultAudioEndpoint = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            }

            foreach (var soundOutRepresenter in SoundOutList)
            {
                soundOutRepresenter.AudioDevices.ToList().ForEach(x => x.IsDefault = false);
                var targetDevice =
                    soundOutRepresenter.AudioDevices.FirstOrDefault(x => x.ID == defaultAudioEndpoint.DeviceID);
                if (targetDevice != null) targetDevice.IsDefault = true;
            }
        }

        void AddDevice(string deviceId)
        {
            foreach (var soundOutRepresenter in SoundOutList.Where(soundOutRepresenter => soundOutRepresenter.AudioDevices.All(x => x.ID != deviceId)))
            {
                soundOutRepresenter.AddDevice(deviceId);
                CheckDefaultAudioDevice(soundOutRepresenter);
            }
            CheckCurrentState();
        }

        void RemoveDevice(string deviceId)
        {
            foreach (var soundOutRepresenter in SoundOutList)
            {
                var audioDevice = soundOutRepresenter.AudioDevices.FirstOrDefault(x => x.ID == deviceId);
                if (audioDevice != null) soundOutRepresenter.AudioDevices.Remove(audioDevice);
                CheckDefaultAudioDevice(soundOutRepresenter);
            }
            CheckCurrentState();
        }

        private bool? _enabled;
        void CheckCurrentState()
        {
            if (SoundOutList.Any(soundOutRepresenter => soundOutRepresenter.AudioDevices.Any(x => x.ID != DefaultDevicePlaceholder)))
            {
                if (_enabled == true) return;
                _enabled = true;
                if (Enable != null) Enable(this, EventArgs.Empty);
                return;
            }

            if (_enabled == true)
            {
                _enabled = false;
                if (Disable != null) Disable(this, EventArgs.Empty);
            }
        }

        #endregion

        #region events

        private string _lastDefaultDeviceChanged;
        void _mmNotificationClient_DefaultDeviceChanged(object sender, DefaultDeviceChangedEventArgs e)
        {
            if (e.DeviceID == _lastDefaultDeviceChanged) return;
            _lastDefaultDeviceChanged = e.DeviceID;
            if (HurricaneSettings.Instance.Config.SoundOutDeviceID == DefaultDevicePlaceholder && e.DeviceID != _currentDeviceId)
            {
                if (RefreshSoundOut != null)
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(OnRefreshSoundOut));
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateDefaultAudioDevice));
        }

        void _mmNotificationClient_DeviceStateChanged(object sender, DeviceStateChangedEventArgs e)
        {
            if (e.DeviceState == DeviceState.Active)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => AddDevice(e.DeviceID)));
            }
            else if (e.DeviceState != DeviceState.Active)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => RemoveDevice(e.DeviceID)));
            }
        }

        void _mmNotificationClient_DeviceRemoved(object sender, DeviceNotificationEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => RemoveDevice(e.DeviceID)));
        }

        void _mmNotificationClient_DeviceAdded(object sender, DeviceNotificationEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => AddDevice(e.DeviceID)));
        }

        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(OnRefreshSoundOut));
        }

        #endregion

        public void Dispose()
        {
            _mmNotificationClient.Dispose();
        }
    }
}