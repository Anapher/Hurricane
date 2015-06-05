using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hurricane.Model.Music.Imagment;

namespace Hurricane.Model.Music.TrackProperties
{
    public class PreviewTrack : INotifyPropertyChanged
    {
        private ImageProvider _image;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }
        public string Artist { get; set; }
        public int Number { get; set; }

        public ImageProvider Image
        {
            get { return _image; }
            set
            {
                if (_image != value)
                {
                    _image = value;
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