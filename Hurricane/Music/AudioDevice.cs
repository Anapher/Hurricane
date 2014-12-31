using System;

namespace Hurricane.Music
{
    [Serializable]
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
}
