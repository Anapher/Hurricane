using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Hurricane.Music
{
    [Serializable]
    public class QueueManager : ObservableCollection<TrackRepresenter>
    {
        protected List<TrackPlaylistPair> trackplaylists;

        public QueueManager()
        {
            trackplaylists = new List<TrackPlaylistPair>();
        }

        public void Initialize(List<Playlist> playlists)
        {
            foreach (var item in this)
            {
                var playlist = item.GetTrack(playlists);
                trackplaylists.Add(new TrackPlaylistPair(item.Track, playlist));
                item.Track.QueueID = (this.IndexOf(item) + 1).ToString();
            }
        }

        public void AddTrack(Track track, Playlist playlist)
        {
            this.Add(new TrackRepresenter(track));
            trackplaylists.Add(new TrackPlaylistPair(track, playlist));
            track.QueueID = (this.IndexOf(GetTrackRepresenter(track)) + 1).ToString();
        }

        protected TrackRepresenter GetTrackRepresenter(Track t)
        {
            var sequenz = this.Where((x) => x.Track == t);
            if (!sequenz.Any()) return null;
            return sequenz.First();
        }

        public void RemoveTrack(Track track)
        {
            var trackrepresentertoremove = GetTrackRepresenter(track);
            if (trackrepresentertoremove == null) return;
            this.Remove(trackrepresentertoremove);
            trackplaylists.Remove(trackplaylists.Where((x) => x.Track == track).First());
            track.QueueID = null;
            RefreshQueuePositionIds();
        }

        public void MoveTrackDown(Track t)
        {
            int trackindex = this.IndexOf(GetTrackRepresenter(t));
            int newindex = trackindex + 1;
            if (newindex > this.Count - 1)
                newindex = 0;
            this.Move(trackindex, newindex);
            RefreshQueuePositionIds();
        }

        public void MoveTrackUp(Track t)
        {
            int trackindex = this.IndexOf(GetTrackRepresenter(t));
            int newindex = trackindex - 1;
            if (newindex < 0) newindex = this.Count - 1;
            this.Move(trackindex, newindex);
            RefreshQueuePositionIds();
        }

        public void ClearTracks()
        {
            this.Clear();
            trackplaylists.Clear();
        }

        public void RefreshQueuePositionIds()
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].Track.QueueID = (i + 1).ToString();
            }
        }

        public Tuple<Track, Playlist> PlayNextTrack()
        {
            if (!HasTracks) return null;
            Track nexttrack = this.First().Track;

            var result = Tuple.Create(nexttrack, this.trackplaylists.Where((x) => x.Track == nexttrack).First().Playlist);
            this.RemoveTrack(nexttrack);
            return result;
        }

        #region Properties
        [XmlIgnore]
        public bool HasTracks
        {
            get
            {
                return this.Count > 0;
            }
        }
        #endregion
    }

   [Serializable] public class TrackRepresenter
    {
        private Track track;
        [XmlIgnore]
        public Track Track
        {
            get { return track; }
            set { track = value; TrackID = value.GenerateHash(); }
        }
        
        public string TrackID { get; set; }

        public Playlist GetTrack(List<Playlist> playlists)
        {
            foreach (var playlist in playlists)
            {
                foreach (var t in playlist.Tracks)
                {
                    if (t.GenerateHash() == TrackID)
                    {
                        this.Track = t;
                        return playlist;
                    }
                }
            }
            return null;
        }

        public TrackRepresenter()
        {
        }

        public TrackRepresenter(Track t)
        {
            this.Track = t;
        }
    }
}
