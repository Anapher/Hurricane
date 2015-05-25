using System.Collections.ObjectModel;

namespace Hurricane.Model.AudioEngine.Engines
{
    public class SoundOutMode : ISoundOutMode
    {
        private readonly GetSoundOutDevice _soundOutDeviceDelegate;

        public delegate ISoundOutDevice GetSoundOutDevice(string deviceId);

        public SoundOutMode(string name, GetSoundOutDevice soundDeviceDelegate)
        {
            Name = name;
            Devices = new ObservableCollection<ISoundOutDevice>();
            _soundOutDeviceDelegate = soundDeviceDelegate;
        }

        public string Name { get; }
        public ObservableCollection<ISoundOutDevice> Devices { get; }

        public void AddDevice(string deviceId)
        {
            var item = _soundOutDeviceDelegate.Invoke(deviceId);
            if (item != null)
                Devices.Add(item);
        }
    }
}