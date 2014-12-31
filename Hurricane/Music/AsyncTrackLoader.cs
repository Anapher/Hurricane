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
            Lists = new List<IList<Track>>();
        }

        public List<IList<Track>> Lists { get; set; }
     
        public void AddTrackList(IList<Track> tracks)
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
                    if (track.NotChecked && !(await track.CheckTrack())) list.Remove(track);
                }
            }
        }
    }
}
