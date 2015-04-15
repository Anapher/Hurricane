using System.Collections;
using System.IO;
using System.Linq;
using System.Windows;
using Hurricane.Music.Download;
using Hurricane.Music.Track;
using Hurricane.ViewModelBase;
using Hurricane.Views;
// ReSharper disable ExplicitCallerInfoArgument

namespace Hurricane.Music
{
    public class MusicManagerCommands
    {
        #region "Constructor"

        protected MusicManager MusicManager;
        public MusicManagerCommands(MusicManager basedmanager)
        {
            MusicManager = basedmanager;
        }

        #endregion

        private RelayCommand _jumptoplayingtrack;
        public RelayCommand JumpToPlayingTrack
        {
            get
            {
                return _jumptoplayingtrack ?? (_jumptoplayingtrack = new RelayCommand(parameter =>
                {
                    if (MusicManager.FavoritePlaylist == MusicManager.CurrentPlaylist)
                        MusicManager.SelectedPlaylist = null;

                    MusicManager.SelectedPlaylist = MusicManager.CurrentPlaylist;
                    MusicManager.SelectedTrack = null;
                    MusicManager.SelectedTrack = MusicManager.CSCoreEngine.CurrentTrack;
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
                    var track = MusicManager.SelectedTrack;
                    if (track == null) return;
                    track.RefreshTrackExists();
                    if (!track.TrackExists) return;
                    track.OpenTrackLocation();
                }));
            }
        }

        private RelayCommand _gobackward;
        public RelayCommand GoBackward
        {
            get
            {
                return _gobackward ??
                       (_gobackward = new RelayCommand(parameter => MusicManager.GoBackward()));
            }
        }

        private RelayCommand _goforward;
        public RelayCommand GoForward
        {
            get
            {
                return _goforward ??
                       (_goforward = new RelayCommand(parameter => MusicManager.GoForward()));
            }
        }

        private RelayCommand _playselectedtrack;
        public RelayCommand PlaySelectedTrack
        {
            get
            {
                return _playselectedtrack ?? (_playselectedtrack = new RelayCommand(parameter =>
                {
                    var track = MusicManager.SelectedTrack;
                    if (track == null) return;
                    if (!track.IsChecked) return;

                    if (track == MusicManager.CSCoreEngine.CurrentTrack)
                        MusicManager.CSCoreEngine.Position = 0;
                    track.RefreshTrackExists();
                    if (track.TrackExists)
                        MusicManager.PlayTrack(track, MusicManager.SelectedPlaylist);
                }));
            }
        }

        private RelayCommand _toggleplaypause;
        public RelayCommand TogglePlayPause
        {
            get
            {
                return _toggleplaypause ?? (_toggleplaypause = new RelayCommand(parameter =>
                {
                    if (MusicManager.CSCoreEngine.CurrentTrack != null)
                    {
                        MusicManager.CSCoreEngine.TogglePlayPause();
                        return;
                    }
                    if (MusicManager.SelectedTrack != null)
                    {
                        MusicManager.PlayTrack(MusicManager.SelectedTrack, MusicManager.SelectedPlaylist);
                        return;
                    }
                    if (MusicManager.SelectedPlaylist.Tracks.Count > 0)
                    {
                        MusicManager.PlayTrack(MusicManager.SelectedPlaylist.Tracks[0], MusicManager.SelectedPlaylist);
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
                    var tracks = ((IList)parameter).Cast<PlayableBase>().Where(x => x.TrackExists).ToList();
                    foreach (var track in tracks.Where(x => !x.IsOpened))
                        MusicManager.Queue.AddTrack(track, MusicManager.SelectedPlaylist);

                    MusicManager.OnPropertyChanged("Queue");
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
                    MusicManager.Queue.RemoveTrack(MusicManager.SelectedTrack);
                    MusicManager.OnPropertyChanged("Queue");
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
                    MusicManager.Queue.ClearTracks();
                    MusicManager.OnPropertyChanged("Queue");
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
                    MusicManager.FavoriteListIsSelected = !MusicManager.FavoriteListIsSelected;
                }));
            }
        }

        private RelayCommand _downloadTracks;
        public RelayCommand DownloadTracks
        {
            get
            {
                return _downloadTracks ?? (_downloadTracks = new RelayCommand(parameter =>
                {
                    if (parameter == null) return;
                    var tracks = ((IList)parameter).Cast<PlayableBase>().ToList().OfType<StreamableBase>().Where(x => x.CanDownload).ToList();
                    if (tracks.Count == 0) return;

                    if (tracks.Count == 1)
                    {
                        var track = tracks[0];

                        var downloadDialog = new DownloadTrackWindow(track.DownloadFilename, DownloadManager.GetExtension(track)) { Owner = Application.Current.MainWindow };
                        if (downloadDialog.ShowDialog() == true)
                        {
                            var settings = downloadDialog.DownloadSettings.Clone();
                            MusicManager.DownloadManager.AddEntry(track, settings, downloadDialog.SelectedPath);
                            MusicManager.DownloadManager.IsOpen = true;
                        }
                    }
                    else
                    {
                        var downloadDialog = new DownloadTrackWindow { Owner = Application.Current.MainWindow };
                        if (downloadDialog.ShowDialog() == true)
                        {
                            var settings = downloadDialog.DownloadSettings.Clone();
                            foreach (var track in tracks)
                            {
                                MusicManager.DownloadManager.AddEntry(track, settings, Path.Combine(downloadDialog.SelectedPath, track.DownloadFilename + settings.GetExtension(track)));
                            }
                            MusicManager.DownloadManager.IsOpen = true;
                        }
                    }
                }));
            }
        }
    }
}