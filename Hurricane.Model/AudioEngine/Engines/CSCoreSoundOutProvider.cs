using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using CSCore.SoundOut.DirectSound;
using Microsoft.Win32;

namespace Hurricane.Model.AudioEngine.Engines
{
    // ReSharper disable once InconsistentNaming
    class CSCoreSoundOutProvider : ISoundOutProvider
    {
        private const string WindowsDefaultId = "DefaultDevice";

        private readonly MMNotificationClient _mmNotificationClient;

        public CSCoreSoundOutProvider()
        {
            _mmNotificationClient = new MMNotificationClient();
            SoundOutModes = new ObservableCollection<ISoundOutMode>();
        }

        public void Dispose()
        {
            _mmNotificationClient.Dispose();
        }

        public event EventHandler InvalidateSoundOut;

        public ObservableCollection<ISoundOutMode> SoundOutModes { get; }
        public bool IsAvailable { get; private set; }

        public void Load()
        {
            _mmNotificationClient.DefaultDeviceChanged += MMNotificationClient_DefaultDeviceChanged;
            _mmNotificationClient.DeviceAdded += MMNotificationClient_DeviceAdded;
            _mmNotificationClient.DeviceRemoved += _mmNotificationClient_DeviceRemoved;
            _mmNotificationClient.DeviceStateChanged += MMNotificationClientOnDeviceStateChanged;

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        public void SetSoundOut(string id)
        {
            throw new NotImplementedException();
        }

        private void LoadSoundOutModes()
        {
            SoundOutModes.Clear();
            using (var enumerator = new MMDeviceEnumerator())
            {
                MMDevice defaultDevice;
                try
                {
                    defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                }
                catch (CoreAudioAPIException)
                {
                    defaultDevice = null;
                }

                if (WasapiOut.IsSupportedOnCurrentPlatform)
                {
                    var wasApiMode = new SoundOutMode("WASAPI", id =>
                    {
                        using (var mmdeviceEnumerator = new MMDeviceEnumerator())
                        {
                            var device =
                                mmdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active)
                                    .FirstOrDefault(x => x.DeviceID == id);
                            return device == null ? null : new SoundOutDevice(device.FriendlyName, device.DeviceID);
                        }
                    });

                    using (var devices = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                    {
                        foreach (var device in devices.Select(x => new SoundOutDevice(x.FriendlyName, x.DeviceID, defaultDevice != null && defaultDevice.DeviceID == x.DeviceID)))
                            wasApiMode.Devices.Add(device);
                    }

                    UpdateWindowsDefault(wasApiMode);
                    SoundOutModes.Add(wasApiMode);
                }

                var directSoundMode = new SoundOutMode("DirectSound", id =>
                {
                    var device =
                        new DirectSoundDeviceEnumerator().Devices
                            .FirstOrDefault(x => x.Guid.ToString() == id);
                    return device == null ? null : new SoundOutDevice(device.Description, device.Guid.ToString());
                });
                foreach (var device in new DirectSoundDeviceEnumerator().Devices.Select(x => new SoundOutDevice(x.Description, x.Guid.ToString(), defaultDevice != null && x.Description == defaultDevice.FriendlyName)))
                    directSoundMode.Devices.Add(device);
                
                UpdateWindowsDefault(directSoundMode);
                SoundOutModes.Add(directSoundMode);
            }
        }

        private void AddDevice(string deviceId)
        {
            foreach (var soundOutMode in SoundOutModes.Where(x => x.Devices.All(y => y.Id != deviceId)))
            {
                ((SoundOutMode) soundOutMode).AddDevice(deviceId);
                UpdateWindowsDefault(soundOutMode);
            }

        }

        private void CheckCurrentState()
        {
            if (SoundOutModes.Any(x => x.Devices.Count > 0))
            {
                if (IsAvailable)
                    return;

                IsAvailable = true;
            }


        }

        private void MMNotificationClient_DeviceAdded(object sender, DeviceNotificationEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _mmNotificationClient_DeviceRemoved(object sender, DeviceNotificationEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MMNotificationClientOnDeviceStateChanged(object sender, DeviceStateChangedEventArgs deviceStateChangedEventArgs)
        {
            
        }

        private void MMNotificationClient_DefaultDeviceChanged(object sender, DefaultDeviceChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                Application.Current.Dispatcher.BeginInvoke(new Action(OnRefreshSoundOut));
        }

        private static void UpdateWindowsDefault(ISoundOutMode soundOutMode)
        {
            if (soundOutMode.Devices.All(x => x.Id == WindowsDefaultId))
            {
                soundOutMode.Devices.Clear(); //No windows default if there aren't any devices
            }
            else if (soundOutMode.Devices.Count > 0 && soundOutMode.Devices.All(x => x.Id != WindowsDefaultId))
            {
                soundOutMode.Devices.Insert(0, new SoundOutDevice("Windows Default", WindowsDefaultId));
            }
        }

        protected void OnRefreshSoundOut()
        {
            InvalidateSoundOut?.Invoke(this, EventArgs.Empty);
        }
    }
}
