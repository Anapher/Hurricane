using Hurricane.Music.Track;
using Hurricane.ViewModelBase;
using System.Collections;
using System.Linq;

namespace Hurricane.Music
{
    class MusicManagerCommands
    {
        #region "Constructor"

        protected MusicManager Musicmanager;
        public MusicManagerCommands(MusicManager basedmanager)
        {
            Musicmanager = basedmanager;
        }

        #endregion

        private RelayCommand _jumptoplayingtrack;
        public RelayCommand JumpToPlayingTrack
        {
            get
            {
                return _jumptoplayingtrack ?? (_jumptoplayingtrack = new RelayCommand(parameter =>
                {
                    Musicmanager.SelectedPlaylist = Musicmanager.CurrentPlaylist;
                    Musicmanager.SelectedTrack = Musicmanager.CSCoreEngine.CurrentTrack;
                }));
            }
        }

        private RelayCommand _opentracklocation;
        public RelayCommand OpenTrackLocation
        {
            get
            {
                return _opentracklocation ?? (_opentracklocation = new RelayCommand(parameter =>
                {
                    if (Musicmanager.SelectedTrack != null)
                        Musicmanager.SelectedTrack.OpenTrackLocation();
                }));
            }
        }

        private RelayCommand _gobackward;
        public RelayCommand GoBackward
        {
            get
            {
                return _gobackward ??
                       (_gobackward = new RelayCommand(parameter => { Musicmanager.GoBackward(); }));
            }
        }

        private RelayCommand _goforward;
        public RelayCommand GoForward
        {
            get
            {
                return _goforward ??
                       (_goforward = new RelayCommand(parameter => { Musicmanager.GoForward(); }));
            }
        }

        private RelayCommand _playselectedtrack;
        public RelayCommand PlaySelectedTrack
        {
            get
            {
                return _playselectedtrack ?? (_playselectedtrack = new RelayCommand(parameter =>
                {
                    var selectedtrack = Musicmanager.SelectedTrack;
                    if (selectedtrack == Musicmanager.CSCoreEngine.CurrentTrack)
                        Musicmanager.CSCoreEngine.Position = 0;
                    selectedtrack.RefreshTrackExists();
                    if (selectedtrack.TrackExists)
                        Musicmanager.PlayTrack(selectedtrack, Musicmanager.SelectedPlaylist);
                }));
            }
        }

        private RelayCommand _toggleplaypause;
        public RelayCommand TogglePlayPause
        {
            get
            {
                return _toggleplaypause ?? (_toggleplaypause = new RelayCommand(async parameter =>
                {
                    if (Musicmanager.CSCoreEngine.CurrentTrack != null)
                    {
                        Musicmanager.CSCoreEngine.TogglePlayPause();
                        return;
                    }
                    if (Musicmanager.SelectedTrack != null)
                    {
                        await Musicmanager.CSCoreEngine.OpenTrack(Musicmanager.SelectedTrack);
                        Musicmanager.CSCoreEngine.TogglePlayPause();
                        return;
                    }
                    if (Musicmanager.SelectedPlaylist.Tracks.Count > 0)
                    {
                        await Musicmanager.CSCoreEngine.OpenTrack(Musicmanager.SelectedPlaylist.Tracks[0]);
                        Musicmanager.CSCoreEngine.TogglePlayPause();
                    }
                }));
            }
        }

        private RelayCommand _addtrackstoqueue;
        public RelayCommand AddTracksToQueue
        {
            get
            {
                return _addtrackstoqueue ?? (_addtrackstoqueue = new RelayCommand(parameter =>
                {
                    if (parameter == null) return;
                    var tracks = ((IList)parameter).Cast<PlayableBase>().ToList();
                    foreach (var track in tracks.Where(x => !x.IsPlaying))
                    {
                        Musicmanager.Queue.AddTrack(track, Musicmanager.SelectedPlaylist);
                    }
                    Musicmanager.OnPropertyChanged("Queue");
                }));
            }
        }

        private RelayCommand _removefromqueue;
        public RelayCommand RemoveFromQueue
        {
            get
            {
                return _removefromqueue ?? (_removefromqueue = new RelayCommand(parameter =>
                {
                    Musicmanager.Queue.RemoveTrack(Musicmanager.SelectedTrack);
                    Musicmanager.OnPropertyChanged("Queue");
                }));
            }
        }

        private RelayCommand _clearqueue;
        public RelayCommand ClearQueue
        {
            get
            {
                return _clearqueue ?? (_clearqueue = new RelayCommand(parameter =>
                {
                    Musicmanager.Queue.ClearTracks();
                    Musicmanager.OnPropertyChanged("Queue");
                }));
            }
        }
            
        private RelayCommand _openFavorites;
        public RelayCommand OpenFavorites
        {
            get
            {
                return _openFavorites ?? (_openFavorites = new RelayCommand(parameter =>
                {
                    Musicmanager.FavoriteListIsSelected = !Musicmanager.FavoriteListIsSelected;
                }));
            }
        }

        private RelayCommand _downloadTrack;
        public RelayCommand DownloadTrack
        {
            get
            {
                return _downloadTrack ?? (_downloadTrack = new RelayCommand(parameter =>
                {
                    var stream = Musicmanager.SelectedTrack as StreamableBase;
                    if (stream == null || !stream.CanDownload) return;
                    Musicmanager.DownloadManager.AddEntry(stream);
                    Musicmanager.DownloadManager.IsOpen = true;
                }));
            }
        }
    }
}
