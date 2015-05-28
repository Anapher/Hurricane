using System.Collections.ObjectModel;
using CSCore.SoundOut;

namespace Hurricane.Model.AudioEngine.Engines
{
    public class SoundOutMode : ISoundOutMode
    {
        private readonly GetSoundOutDevice _soundOutDeviceDelegate;
        private readonly ISoundOutDevice _windowsDefaultDevice;
        private readonly GetISoundOut _getISoundOut;

        public delegate ISoundOutDevice GetSoundOutDevice(string deviceId);
        public delegate ISoundOut GetISoundOut(ISoundOutDevice device);

        public SoundOutMode(string name, SoundOutType type, GetSoundOutDevice soundDeviceDelegate, GetISoundOut getISoundOutDelegate, ISoundOutDevice windowsDefaultDevice)
        {
            Name = name;
            Devices = new ObservableCollection<ISoundOutDevice>();
            _soundOutDeviceDelegate = soundDeviceDelegate;
            _windowsDefaultDevice = windowsDefaultDevice;
            _getISoundOut = getISoundOutDelegate;
            SoundOutType = type;
        }

        public string Name { get; }
        public ObservableCollection<ISoundOutDevice> Devices { get; }
        public SoundOutType SoundOutType { get; }

        public void AddDevice(string deviceId)
        {
            var item = _soundOutDeviceDelegate.Invoke(deviceId);
            if (item != null)
                Devices.Add(item);
        }

        public void AddWindowsDefaultDevice()
        {
            Devices.Insert(0, _windowsDefaultDevice);
        }

        public ISoundOut GetSoundOut(ISoundOutDevice device)
        {
            return _getISoundOut(device);
        }
    }

    public enum SoundOutType { WasApi, DirectSound }
}