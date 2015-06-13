using System;
using System.Globalization;
using System.Threading.Tasks;
using Hurricane.Model;
using Hurricane.Model.Music;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.Model.Notifications;
using Hurricane.ViewModel.MainView.Base;

namespace Hurricane.ViewModel.MainView
{
    public class ArtistView : PropertyChangedBase, IViewItem
    {
        private MusicDataManager _musicDataManager;
        private readonly Action _closeView;
        private bool _isLoaded;
        private ViewController _viewController;

        private RelayCommand _closeCommand;
        private RelayCommand _openArtistCommand;

        public ArtistView(Artist artist, Action closeView)
        {
            Artist = artist;
            _closeView = closeView;
        }

        public bool IsPlaying { get; set; }
        public Artist Artist { get; }

        public RelayCommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(parameter => { _closeView.Invoke(); })); }
        }

        public RelayCommand OpenArtistCommand
        {
            get
            {
                return _openArtistCommand ?? (_openArtistCommand = new RelayCommand(parameter =>
                {
                    _viewController.OpenArtist((Artist) parameter);
                }));
            }
        }

        public async Task Load(MusicDataManager musicDataManager, ViewController viewController, NotificationManager notificationManager)
        {
            if (!_isLoaded)
            {
                _viewController = viewController;
                _musicDataManager = musicDataManager;
                await _musicDataManager.LastfmApi.SetAdvancedInfoAboutArtist(Artist, CultureInfo.CurrentCulture);
                _isLoaded = true;
            }
        }
    }
}