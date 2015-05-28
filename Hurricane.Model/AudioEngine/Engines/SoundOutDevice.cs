using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hurricane.Model.AudioEngine.Engines
{
    public class SoundOutDevice : ISoundOutDevice, INotifyPropertyChanged
    {
        private bool _isDefault;

        public SoundOutDevice(string name, string id, SoundOutType soundOutType, bool isDefault)
        {
            Name = name;
            Id = id;
            IsDefault = isDefault;
            Type = soundOutType;
        }

        public SoundOutDevice(string name, string id, SoundOutType soundOutType) : this(name, id, soundOutType, false)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; }
        public string Id { get; }
        public SoundOutType Type { get; }
        public bool IsDefault
        {
            get { return _isDefault; }
            set
            {
                if (value != _isDefault)
                {
                    _isDefault = value;
                    OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}