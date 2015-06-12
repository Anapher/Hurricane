using System;
using Hurricane.Model.Music.TrackProperties;

namespace Hurricane.ViewModel.MainView
{
    public class ViewController
    {
        private readonly Action<Artist> _openArtistAction; 

        public ViewController(Action<Artist> openArtistAction)
        {
            _openArtistAction = openArtistAction;
        }

        public void OpenArtist(Artist artist)
        {
            _openArtistAction.Invoke(artist);
        }
    }
}