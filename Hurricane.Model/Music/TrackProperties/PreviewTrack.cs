using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace Hurricane.Model.Music.TrackProperties
{
    public class PreviewTrack : INotifyPropertyChanged
    {
        private BitmapImage _image;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }
        public string Artist { get; set; }
        public int Number { get; set; }
        public string ImageUrl { get; set; }

        public BitmapImage Image
        {
            get
            {
                return _image;
            }
            set
            {
                if (!Equals(_image, value))
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