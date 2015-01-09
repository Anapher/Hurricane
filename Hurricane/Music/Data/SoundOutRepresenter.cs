using System.Collections.Generic;
using Hurricane.Settings;

namespace Hurricane.Music.Data
{
    public class SoundOutRepresenter
    {
        public string Name { get; set; }
        public List<AudioDevice> AudioDevices { get; set; }
        public SoundOutMode SoundOutMode { get; set; }

        public SoundOutRepresenter()
        {
            AudioDevices = new List<AudioDevice>();
        }
    }
}
