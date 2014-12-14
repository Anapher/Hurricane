using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Hurricane.Music
{
    [Serializable]
    public class QueueManager : ObservableCollection<TrackRepresenter>
    {
        [XmlIgnore]
        public ObservableCollection<TrackPlaylistPair> TrackPlaylists { get; set; }

        public QueueManager()
        {
            TrackPlaylists = new ObservableCollection<TrackPlaylistPair>();
        }

        public void Initialize(List<Playlist> playlists)
        {
            foreach (var item in this)
            {
                var playlist = item.GetTrack(playlists);
                TrackPlaylists.Add(new TrackPlaylistPair(item.Track, playlist));
                item.Track.QueueID = (this.IndexOf(item) + 1).ToString();
            }
            RefreshDuration();
        }

        public void AddTrack(Track track, Playlist playlist)
        {
            this.Add(new TrackRepresenter(track));
            TrackPlaylists.Add(new TrackPlaylistPair(track, playlist));
            track.QueueID = (this.IndexOf(GetTrackRepresenter(track)) + 1).ToString();
            RefreshDuration();
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
            TrackPlaylists.Remove(TrackPlaylists.Where((x) => x.Track == track).First());
            track.QueueID = null;
            RefreshQueuePositionIds();
            RefreshDuration();
        }

        #region TrackMoving
        public void MoveTrackDown(TrackRepresenter t, int count)
        {
            int trackindex = this.IndexOf(t);
            int newindex = trackindex + count;
            if (newindex > this.Count - 1)
                newindex = 0;
            this.Move(trackindex, newindex);
            TrackPlaylists.Move(trackindex, newindex);
            RefreshQueuePositionIds();
        }

        public void MoveTrackUp(TrackRepresenter t, int count)
        {
            int trackindex = this.IndexOf(t);
            int newindex = trackindex - count;
            if (newindex < 0) newindex = this.Count - 1;
            this.Move(trackindex, newindex);
            TrackPlaylists.Move(trackindex, newindex);
            RefreshQueuePositionIds();
        }

        public void MoveTrackDown(Track t, int count)
        {
            this.MoveTrackDown(GetTrackRepresenter(t), count);
        }

        public void MoveTrackUp(Track t, int count)
        {
            this.MoveTrackUp(GetTrackRepresenter(t), count);
        }

        public void MoveTrackDown(TrackRepresenter t)
        {
            this.MoveTrackDown(t, 1);
        }

        public void MoveTrackUp(TrackRepresenter t)
        {
            this.MoveTrackUp(t, 1);
        }

        public void MoveTrackDown(Track t)
        {
            this.MoveTrackDown(GetTrackRepresenter(t), 1);
        }

        public void MoveTrackUp(Track t)
        {
            this.MoveTrackUp(GetTrackRepresenter(t));
        }

        #endregion

        public int IndexOf(Track t)
        {
            return this.IndexOf(GetTrackRepresenter(t));
        }

        public void ClearTracks()
        {
            foreach (var item in this)
            {
                item.Track.QueueID = null;    
            }
            this.Clear();
            TrackPlaylists.Clear();
            RefreshQueuePositionIds();
            RefreshDuration();
        }

        protected void RefreshDuration()
        {
            TimeSpan newduration = TimeSpan.Zero;
            foreach (var item in this)
            {
                newduration += item.Track.DurationTimespan;
            }
            this.Duration = newduration;
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

            var result = Tuple.Create(nexttrack, this.TrackPlaylists.Where((x) => x.Track == nexttrack).First().Playlist);
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

        private TimeSpan duration;
        public TimeSpan Duration
        {
            get { return duration; }
            set { duration = value; OnPropertyChanged(new PropertyChangedEventArgs("Duration")); }
        }
        
        #endregion
    }

    [Serializable]
    public class TrackRepresenter
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
