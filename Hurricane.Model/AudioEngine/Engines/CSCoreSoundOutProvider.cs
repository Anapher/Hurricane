using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CSCore.CoreAudioAPI;
using CSCore.DirectSound;
using CSCore.SoundOut;
using Microsoft.Win32;

namespace Hurricane.Model.AudioEngine.Engines
{
    // ReSharper disable once InconsistentNaming
    class CSCoreSoundOutProvider : ISoundOutProvider
    {
        private const string WindowsDefaultId = "DefaultDevice";

        private readonly MMNotificationClient _mmNotificationClient;
        private string _lastDefaultDeviceChanged;
        private ISoundOutDevice _currentSoundOutDevice;

        public CSCoreSoundOutProvider()
        {
            _mmNotificationClient = new MMNotificationClient();
            SoundOutModes = new ObservableCollection<ISoundOutMode>();
            LoadSoundOutModes();
            LoadEvents();
        }

        public void Dispose()
        {
            _mmNotificationClient.Dispose();
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        }

        public event EventHandler InvalidateSoundOut;
        public event EventHandler Enable;
        public event EventHandler Disable;

        public ObservableCollection<ISoundOutMode> SoundOutModes { get; }
        public bool IsAvailable { get; private set; }

        public ISoundOutDevice CurrentSoundOutDevice
        {
            get { return _currentSoundOutDevice; }
            set
            {
                if (_currentSoundOutDevice != value)
                {
                    _currentSoundOutDevice = value;
                    OnInvalidateSoundOut();
                }
            }
        }

        private void LoadEvents()
        {
            _mmNotificationClient.DefaultDeviceChanged += MMNotificationClient_DefaultDeviceChanged;
            _mmNotificationClient.DeviceAdded += MMNotificationClient_DeviceAdded;
            _mmNotificationClient.DeviceRemoved += MMNotificationClient_DeviceRemoved;
            _mmNotificationClient.DeviceStateChanged += MMNotificationClientOnDeviceStateChanged;

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        public ISoundOut GetSoundOut()
        {
            if (!IsAvailable)
                return null;

            if (CurrentSoundOutDevice == null)
            {
                foreach (var soundOutMode in SoundOutModes)
                {
                    if (soundOutMode.Devices.Count == 0) break;
                    _currentSoundOutDevice = soundOutMode.Devices[0];
                    break;
                }

                if (_currentSoundOutDevice == null)
                {
                    IsAvailable = false;
                    Disable?.Invoke(this, EventArgs.Empty);
                    return null;
                }
            }

            foreach (var mode in SoundOutModes)
            {
                var soundOutMode = (SoundOutMode) mode;
                // ReSharper disable once PossibleNullReferenceException
                if (soundOutMode.SoundOutType == ((SoundOutDevice) CurrentSoundOutDevice).Type)
                {
                    try
                    {
                        return soundOutMode.GetSoundOut(CurrentSoundOutDevice);
                    }
                    catch (NoDeviceFoundException)
                    {
                    }
                }
            }

            IsAvailable = false;
            Disable?.Invoke(this, EventArgs.Empty);
            return null;
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
                    var wasApiMode = new SoundOutMode("WASAPI", SoundOutType.WasApi, GetWasApiSoundOutDeviceById, GetWasApiSoundOut, new SoundOutDevice("Windows Default", WindowsDefaultId, SoundOutType.WasApi));

                    using (var devices = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                    {
                        foreach (var device in devices.Select(x => new SoundOutDevice(x.FriendlyName, x.DeviceID, SoundOutType.WasApi, defaultDevice != null && defaultDevice.DeviceID == x.DeviceID)))
                            wasApiMode.Devices.Add(device);
                    }

                    UpdateWindowsDefault(wasApiMode);
                    SoundOutModes.Add(wasApiMode);
                }

                var directSoundMode = new SoundOutMode("DirectSound", SoundOutType.DirectSound, GetDirectSoundOutDeviceById, GetDirectSoundOut, new SoundOutDevice("Windows Default", WindowsDefaultId, SoundOutType.DirectSound));
                foreach (var device in DirectSoundDeviceEnumerator.EnumerateDevices().Select(x => new SoundOutDevice(x.Description, x.Guid.ToString(), SoundOutType.DirectSound, defaultDevice != null && x.Description == defaultDevice.FriendlyName)))
                    directSoundMode.Devices.Add(device);

                UpdateWindowsDefault(directSoundMode);
                SoundOutModes.Add(directSoundMode);
                CheckCurrentState();
            }
        }

        private static ISoundOutDevice GetWasApiSoundOutDeviceById(string id)
        {
            using (var mmdeviceEnumerator = new MMDeviceEnumerator())
            {
                using (var device =
                    mmdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active)
                        .FirstOrDefault(x => x.DeviceID == id))
                {
                    return device == null
                        ? null
                        : new SoundOutDevice(device.FriendlyName, device.DeviceID,
                            SoundOutType.WasApi);
                }
            }
        }

        private static ISoundOut GetWasApiSoundOut(ISoundOutDevice device)
        {
            MMDevice mmDevice = null;
            if (device.Id == WindowsDefaultId)
            {
                mmDevice = GetDefaultDevice();
            }

            if (mmDevice == null)
            {
                using (var enumerator = new MMDeviceEnumerator())
                {
                    using (var devices = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                    {
                        if (devices == null || devices.Count == 0)
                            throw new NoDeviceFoundException();

                        mmDevice = devices.FirstOrDefault(x => x.DeviceID == device.Id) ?? GetDefaultDevice() ?? devices.First();
                    }
                }
            }

            return new WasapiOut { Device = mmDevice };
        }

        private static ISoundOutDevice GetDirectSoundOutDeviceById(string id)
        {
            var device =
                DirectSoundDeviceEnumerator.EnumerateDevices()
                    .FirstOrDefault(x => x.Guid.ToString() == id);
            return device == null
                ? null
                : new SoundOutDevice(device.Description, device.Guid.ToString(),
                    SoundOutType.DirectSound);
        }

        private static ISoundOut GetDirectSoundOut(ISoundOutDevice device)
        {
            var devices = DirectSoundDeviceEnumerator.EnumerateDevices();
            if (devices.Count == 0)
                throw new NoDeviceFoundException();

            DirectSoundDevice directSoundDevice = null;

            if (device.Id == WindowsDefaultId)
            {
                var defaultAudioId = GetDefaultDevice().FriendlyName;
                directSoundDevice = devices.FirstOrDefault(x => x.Description == defaultAudioId);
            }

            if (directSoundDevice == null)
            {
                directSoundDevice = devices.FirstOrDefault(x => x.Guid.ToString() == device.Id) ?? devices.First();
            }

            return new DirectSoundOut { Device = directSoundDevice.Guid };
        }

        private void MMNotificationClient_DeviceAdded(object sender, DeviceNotificationEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => AddDevice(e.DeviceId)));
        }

        private void MMNotificationClient_DeviceRemoved(object sender, DeviceNotificationEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => RemoveDevice(e.DeviceId)));
        }

        // ReSharper disable InconsistentNaming
        private void MMNotificationClientOnDeviceStateChanged(object sender, DeviceStateChangedEventArgs e)
        // ReSharper restore InconsistentNaming
        {
            if (e.DeviceState == DeviceState.Active)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => AddDevice(e.DeviceId)));
            }
            else if (e.DeviceState != DeviceState.Active)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => RemoveDevice(e.DeviceId)));
            }
        }

        private void MMNotificationClient_DefaultDeviceChanged(object sender, DefaultDeviceChangedEventArgs e)
        {
            if (e.DeviceId == _lastDefaultDeviceChanged)
                return;

            _lastDefaultDeviceChanged = e.DeviceId;
            if (CurrentSoundOutDevice.Id == WindowsDefaultId)
                Application.Current.Dispatcher.BeginInvoke(new Action(OnInvalidateSoundOut));

            Application.Current.Dispatcher.BeginInvoke(new Action(UpdateDefaultAudioDevice));
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                Application.Current.Dispatcher.BeginInvoke(new Action(OnInvalidateSoundOut));
        }

        private void AddDevice(string deviceId)
        {
            foreach (var soundOutMode in SoundOutModes.Where(x => x.Devices.All(y => y.Id != deviceId)))
            {
                ((SoundOutMode)soundOutMode).AddDevice(deviceId);
                UpdateWindowsDefault(soundOutMode);
            }
            CheckCurrentState();
        }

        private void RemoveDevice(string deviceId)
        {
            foreach (var soundOutMode in SoundOutModes)
            {
                var device = soundOutMode.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (device == null)
                    continue;

                soundOutMode.Devices.Remove(device);
                UpdateWindowsDefault(soundOutMode);
            }
            CheckCurrentState();
        }

        private void CheckCurrentState()
        {
            if (SoundOutModes.Any(x => x.Devices.Count > 0))
            {
                if (IsAvailable)
                    return;

                IsAvailable = true;
                Enable?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (!IsAvailable) return;
            IsAvailable = false;
            Disable?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateDefaultAudioDevice()
        {
            MMDevice defaultAudioEndpoint;
            using (var enumerator = new MMDeviceEnumerator())
            {
                defaultAudioEndpoint = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            }

            foreach (var soundOutMode in SoundOutModes)
            {
                foreach (var soundOutDevice in soundOutMode.Devices)
                    soundOutDevice.IsDefault = false;

                var newDefaultDevice = soundOutMode.Devices.FirstOrDefault(x => x.Id == defaultAudioEndpoint.DeviceID);
                if (newDefaultDevice != null)
                    newDefaultDevice.IsDefault = true;
            }
        }

        private static MMDevice GetDefaultDevice()
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                try
                {
                    return enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                }
                catch (CoreAudioAPIException)
                {
                    return null;
                }
            }
        }

        private static void UpdateWindowsDefault(ISoundOutMode soundOutMode)
        {
            if (soundOutMode.Devices.All(x => x.Id == WindowsDefaultId))
            {
                soundOutMode.Devices.Clear(); //No windows default if there aren't any devices
            }
            else if (soundOutMode.Devices.Count > 0 && soundOutMode.Devices.All(x => x.Id != WindowsDefaultId))
            {
                ((SoundOutMode)soundOutMode).AddWindowsDefaultDevice();
            }
        }

        protected void OnInvalidateSoundOut()
        {
            InvalidateSoundOut?.Invoke(this, EventArgs.Empty);
        }
    }
}