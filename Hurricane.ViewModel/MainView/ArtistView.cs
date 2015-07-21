using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Hurricane.Model;
using Hurricane.Model.Music;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.Model.Notifications;
using Hurricane.Model.Services;
using Hurricane.Utilities;
using Hurricane.ViewModel.MainView.Base;

namespace Hurricane.ViewModel.MainView
{
    public class ArtistView : PropertyChangedBase, IViewItem, IPlaylist
    {
        private MusicDataManager _musicDataManager;
        private readonly Action _closeView;
        private bool _isLoaded;
        private ViewController _viewController;

        private RelayCommand _closeCommand;
        private RelayCommand _openArtistCommand;
        private RelayCommand _playTrackCommand;

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

        public RelayCommand PlayTrackCommand
        {
            get
            {
                return _playTrackCommand ?? (_playTrackCommand = new RelayCommand(async parameter =>
                {
                    var track = parameter as TopTrack;
                    if (track == null)
                        return;

                    _viewController.SetIsPlaying(null);
                    _musicDataManager.MusicManager.OpenPlayable(await GetPlayable(track), this).Forget();
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

        private async Task<IPlayable> GetPlayable(TopTrack track)
        {
            var result = await _musicDataManager.SearchTrack(Artist, track.Name);
            result.Tag = track;

            var searchResult = result as ISearchResult;
            if (searchResult != null)
            {
                searchResult.Title = track.Name;
                searchResult.Artist = Artist.Name;
                searchResult.Cover = track.Thumbnail;
            }

            return result;
        }

        Task<IPlayable> IPlaylist.GetNextTrack(IPlayable currentTrack)
        {
            return GetPlayable(Artist.TopTracks.GetNextObject(currentTrack));
        }

        Task<IPlayable> IPlaylist.GetShuffleTrack()
        {
            return GetPlayable(Artist.TopTracks.GetRandomObject());
        }

        Task<IPlayable> IPlaylist.GetPreviousTrack(IPlayable currentTrack)
        {
            return GetPlayable(Artist.TopTracks.GetPreviousObject(currentTrack));
        }

        Task<IPlayable> IPlaylist.GetLastTrack()
        {
            return GetPlayable(Artist.TopTracks.Last());
        }

        bool IPlaylist.ContainsPlayableTracks()
        {
            return Artist.TopTracks.Count > 0;
        }
    }
}