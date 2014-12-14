using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurricane.ViewModelBase;

namespace Hurricane.Music
{
    class MusicManagerCommands
    {
        #region "Constructor"

        protected MusicManager musicmanager;
        public MusicManagerCommands(MusicManager basedmanager)
        {
            this.musicmanager = basedmanager;
        }

        #endregion

        private RelayCommand jumptoplayingtrack;
        public RelayCommand JumpToPlayingTrack
        {
            get
            {
                if (jumptoplayingtrack == null)
                    jumptoplayingtrack = new RelayCommand((object parameter) =>
                    {
                        musicmanager.SelectedPlaylist = musicmanager.CurrentPlaylist;
                        musicmanager.SelectedTrack = musicmanager.CSCoreEngine.CurrentTrack;
                    });
                return jumptoplayingtrack;
            }
        }

        private RelayCommand opentracklocation;
        public RelayCommand OpenTrackLocation
        {
            get
            {
                if (opentracklocation == null)
                    opentracklocation = new RelayCommand((object parameter) => { System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + musicmanager.SelectedTrack.Path + "\""); });
                return opentracklocation;
            }
        }

        private RelayCommand gobackward;
        public RelayCommand GoBackward
        {
            get
            {
                if (gobackward == null)
                    gobackward = new RelayCommand((object parameter) => { musicmanager.GoBackward(); });
                return gobackward;
            }
        }

        private RelayCommand goforward;
        public RelayCommand GoForward
        {
            get
            {
                if (goforward == null)
                    goforward = new RelayCommand((object parameter) => { musicmanager.GoForward(); });
                return goforward;
            }
        }

        private RelayCommand playselectedtrack;
        public RelayCommand PlaySelectedTrack
        {
            get
            {
                if (playselectedtrack == null)
                    playselectedtrack = new RelayCommand((object parameter) =>
                    {
                        var selectedtrack = musicmanager.SelectedTrack;
                        selectedtrack.RefreshTrackExists();
                        if (selectedtrack != musicmanager.CSCoreEngine.CurrentTrack && selectedtrack.TrackExists)
                        {
                            musicmanager.PlayTrack(selectedtrack, musicmanager.SelectedPlaylist);
                        }
                    });
                return playselectedtrack;
            }
        }

        private RelayCommand toggleplaypause;
        public RelayCommand TogglePlayPause
        {
            get
            {
                if (toggleplaypause == null)
                    toggleplaypause = new RelayCommand((object parameter) => { musicmanager.CSCoreEngine.TogglePlayPause(); });
                return toggleplaypause;
            }
        }

        private RelayCommand addtracktoqueue;
        public RelayCommand AddTrackToQueue
        {
            get
            {
                if (addtracktoqueue == null)
                    addtracktoqueue = new RelayCommand((object parameter) => { musicmanager.Queue.AddTrack(musicmanager.SelectedTrack, musicmanager.SelectedPlaylist); musicmanager.OnPropertyChanged("Queue"); });
                return addtracktoqueue;
            }
        }

        private RelayCommand removefromqueue;
        public RelayCommand RemoveFromQueue
        {
            get
            {
                if (removefromqueue == null)
                    removefromqueue = new RelayCommand((object parameter) => { musicmanager.Queue.RemoveTrack(musicmanager.SelectedTrack); musicmanager.OnPropertyChanged("Queue"); });
                return removefromqueue;
            }
        }

        private RelayCommand clearqueue;
        public RelayCommand ClearQueue
        {
            get
            {
                if (clearqueue == null)
                    clearqueue = new RelayCommand((object parameter) => { musicmanager.Queue.ClearTracks(); musicmanager.OnPropertyChanged("Queue"); });
                return clearqueue;
            }
        }
    }
}
