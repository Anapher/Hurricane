using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hurricane.Music.Playlist;

namespace Hurricane.Music
{
    class AsyncTrackLoader
    {
        private static AsyncTrackLoader _instance;
        public static AsyncTrackLoader Instance
        {
            get { return _instance ?? (_instance = new AsyncTrackLoader()); }
        }

        private AsyncTrackLoader()
        {
            PlaylistsToCheck = new List<IPlaylist>();
        }

        private bool _isrunning;

        public List<IPlaylist> PlaylistsToCheck { get; set; }

        private bool _havetocheck;
        public void RunAsync(List<IPlaylist> lst)
        {
            PlaylistsToCheck.AddRange(lst.Where(p => !PlaylistsToCheck.Contains(p))); //We only add tracks which aren't already in our queue
            Run();
        }

        public void RunAsync(IPlaylist lst)
        {
            PlaylistsToCheck.Add(lst);
            Run();
        }

        private void Run()
        {
            _havetocheck = true;
            var t = LoadTracks();
        }

        protected async Task LoadTracks()
        {
            if (_isrunning) return;
            _isrunning = true;
            _havetocheck = false;
            foreach (var p in PlaylistsToCheck.ToList())
            {
                PlaylistsToCheck.Remove(p);
                foreach (var track in p.Tracks.Where(x => x.TrackExists && !x.IsChecked).ToList())
                {
                    if (!await track.CheckTrack())
                    {
                        if (p.Tracks.Contains(track)) p.RemoveTrack(track); //if the user removed the track
                    }
                }
            }

            _isrunning = false;
            if (_havetocheck) await LoadTracks();
        }
    }
}