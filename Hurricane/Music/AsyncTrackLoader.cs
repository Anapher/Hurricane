using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Lists = new List<IList<TrackPlaylistPair>>();
        }

        public List<IList<TrackPlaylistPair>> Lists { get; set; }

        public void AddTrackList(IList<TrackPlaylistPair> tracks)
        {
            Lists.Add(tracks);
        }

        protected bool TracksAreLoaded;
        protected async void LoadTracks()
        {
            TracksAreLoaded = true;
            foreach (var list in Lists)
            {
                foreach (var track in list)
                {
                    if (track.Track.NotChecked && !(await track.Track.CheckTrack())) track.Playlist.RemoveTrackWithAnimation(track.Track);
                }
            }
        }
    }
}
