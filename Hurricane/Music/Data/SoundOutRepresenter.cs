using System.Collections.ObjectModel;
using Hurricane.Settings;

namespace Hurricane.Music.Data
{
    public class SoundOutRepresenter
    {
        public delegate AudioDevice GetAudioDevice(string deviceId);

        public string Name { get; set; }
        public ObservableCollection<AudioDevice> AudioDevices { get; set; }
        public SoundOutMode SoundOutMode { get; set; }

        private readonly GetAudioDevice _getAudioDeviceAction;
        public SoundOutRepresenter(GetAudioDevice audioDeviceAction)
        {
            AudioDevices = new ObservableCollection<AudioDevice>();
            _getAudioDeviceAction = audioDeviceAction;
        }

        public void AddDevice(string deviceID)
        {
            var newItem = _getAudioDeviceAction.Invoke(deviceID);
            if (newItem != null) AudioDevices.Add(newItem);
        }
    }
}