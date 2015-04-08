using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Hurricane.Music;
using Hurricane.Music.Playlist;
using Hurricane.Music.Track;
using Hurricane.Settings;

namespace Hurricane.Views.MetroDialogs
{
    /// <summary>
    /// Interaction logic for AddCustomStreamView.xaml
    /// </summary>
    public partial class AddCustomStreamView : INotifyPropertyChanged
    {
        private readonly Action<AddCustomStreamView> _closeAction;
        private readonly NormalPlaylist _playlist;
        private readonly MusicManager _manager;

        public AddCustomStreamView(NormalPlaylist playlist, MusicManager manager, Action<AddCustomStreamView> closeAction)
        {
            InitializeComponent();
            _closeAction = closeAction;
            _playlist = playlist;
            _manager = manager;
        }

        private bool _isChecking;
        public bool IsChecking
        {
            get { return _isChecking; }
            set
            {
                if (value != _isChecking)
                {
                    _isChecking = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _canAddTrack;
        public bool CanAddTrack
        {
            get { return _canAddTrack; }
            set
            {
                if (value != _canAddTrack)
                {
                    _canAddTrack = value;
                    OnPropertyChanged();
                }
            }
        }

        private CustomStream _currentTrack;
        public CustomStream CurrentTrack
        {
            get { return _currentTrack; }
            set
            {
                if (value != _currentTrack)
                {
                    _currentTrack = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _streamUrl;
        public string StreamUrl
        {
            get { return _streamUrl; }
            set
            {
                _streamUrl = value;
                OnPropertyChanged();
            }
        }


        private async void Check_OnClick(object sender, RoutedEventArgs e)
        {
            IsChecking = true;
            CurrentTrack = new CustomStream { StreamUrl = StreamUrl };
            if (await CurrentTrack.CheckTrack())
            {
                CanAddTrack = true;
            }
            else
            {
                System.Media.SystemSounds.Hand.Play();
                CanAddTrack = false;
            }

            IsChecking = false;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            _closeAction.Invoke(this);
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            CurrentTrack.IsChecked = false;
            CurrentTrack.TimeAdded = DateTime.Now;

            _playlist.AddTrack(CurrentTrack);

            _manager.SaveToSettings();
            HurricaneSettings.Instance.Save();
            AsyncTrackLoader.Instance.RunAsync(_playlist);
            _closeAction.Invoke(this);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;


    }
}