using Hurricane.ViewModelBase;

namespace Hurricane.Music.Data
{
    public class AudioDevice : PropertyChangedBase
    {
        public string ID { get; set; }
        public string Name { get; set; }
        
        private bool _isDefault;
        public bool IsDefault
        {
            get { return _isDefault; }
            set
            {
                SetProperty(value, ref _isDefault);
            }
        }

        public AudioDevice(string id, string name, bool isDefault = false)
        {
            ID = id;
            Name = name;
            IsDefault = isDefault;
        }

        public override string ToString()
        {
            return IsDefault ? Name + " (Default)" : Name;
        }
    }
}
