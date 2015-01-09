using System;

namespace Hurricane.Music.Data
{
    public class AudioDevice
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }

        public AudioDevice(string id, string name, bool isDefault = false)
        {
            ID = id;
            Name = name;
            IsDefault = isDefault;
        }

        public override string ToString()
        {
            return this.IsDefault ? Name + " (Default)" : Name;
        }
    }
}
