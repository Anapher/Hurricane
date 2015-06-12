using System;
using System.Globalization;
using Hurricane.Model.Music;
using Hurricane.Model.Music.TrackProperties;

namespace Hurricane.ViewModel.MainView
{
    public class ArtistView
    {
        private readonly MusicDataManager _musicDataManager;
        private readonly Action _closeView;

        private RelayCommand _closeCommand;

        public ArtistView(Artist artist, MusicDataManager musicDataManager, Action closeView)
        {
            Artist = artist;
            _musicDataManager = musicDataManager;
            _closeView = closeView;
            Initalize();
        }

        private async void Initalize()
        {
            await _musicDataManager.LastfmApi.SetAdvancedInfoAboutArtist(Artist, CultureInfo.CurrentCulture);
        }

        public Artist Artist { get; }

        public RelayCommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(parameter => { _closeView.Invoke(); })); }
        }
    }
}