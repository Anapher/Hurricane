using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hurricane.Music.Playlist;
using Hurricane.Music.Track;

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
            PlaylistsToCheck = new List<NormalPlaylist>();
        }

        private bool _isrunning;

        public List<NormalPlaylist> PlaylistsToCheck { get; set; }

        private bool havetocheck;
        public void RunAsync(List<NormalPlaylist> lst)
        {
            PlaylistsToCheck.AddRange(lst.Where(p => !PlaylistsToCheck.Contains(p))); //We only add tracks which aren't already in our queue
            havetocheck = true;
            var t = LoadTracks();
        }

        protected async Task LoadTracks()
        {
            if (_isrunning) return;
            _isrunning = true;
            havetocheck = false;
            foreach (var p in PlaylistsToCheck.ToList())
            {
                foreach (var track in p.Tracks.OfType<LocalTrack>().Where(x => x.TrackExists))
                {
                    if(track.IsChecked == false && !await track.CheckTrack()) p.RemoveTrack(track);
                }
                PlaylistsToCheck.Remove(p);
            }

            _isrunning = false;
            if (havetocheck) await LoadTracks();
        }
    }
}
