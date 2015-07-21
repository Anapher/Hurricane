using System;
using Hurricane.Model.Music.TrackProperties;

namespace Hurricane.ViewModel.MainView.Base
{
    public class ViewController
    {
        private readonly Action<Artist> _openArtistAction;
        private IViewItem _currentlyPlayingView;

        public ViewController(Action<Artist> openArtistAction)
        {
            _openArtistAction = openArtistAction;
        }

        public void OpenArtist(Artist artist)
        {
            _openArtistAction.Invoke(artist);
        }

        public void SetIsPlaying(IViewItem viewItem)
        {
            if (viewItem == _currentlyPlayingView)
                return;

            if (_currentlyPlayingView != null)
                _currentlyPlayingView.IsPlaying = false;

            if (viewItem != null)
                viewItem.IsPlaying = true;

            _currentlyPlayingView = viewItem;
        }
    }
}