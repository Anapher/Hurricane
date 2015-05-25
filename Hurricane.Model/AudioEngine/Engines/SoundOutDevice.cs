using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hurricane.Model.AudioEngine.Engines
{
    class SoundOutDevice : ISoundOutDevice, INotifyPropertyChanged
    {
        private bool _isDefault;

        public SoundOutDevice(string name, string id, bool isDefault)
        {
            Name = name;
            Id = id;
            IsDefault = isDefault;
        }

        public SoundOutDevice(string name, string id) : this(name, id, false)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; }
        public string Id { get; }
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
