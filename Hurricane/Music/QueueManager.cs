using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Hurricane.Music.Data;

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

        public void Initialize(IEnumerable<Playlist> playlists)
        {
            foreach (var item in this)
            {
                var playlist = item.GetTrack(playlists);
                TrackPlaylists.Add(new TrackPlaylistPair(item.Track, playlist));
                item.Track.QueueID = (IndexOf(item) + 1).ToString();
            }
            RefreshDuration();
        }

        public void AddTrack(Track track, IPlaylist playlist)
        {
            Add(new TrackRepresenter(track));
            TrackPlaylists.Add(new TrackPlaylistPair(track, playlist));
            track.QueueID = (IndexOf(GetTrackRepresenter(track)) + 1).ToString();
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
            TrackPlaylists.Remove(TrackPlaylists.First(x => x.Track == track));
            track.QueueID = null;
            RefreshQueuePositionIds();
            RefreshDuration();
        }

        #region TrackMoving
        public void MoveTrackDown(TrackRepresenter t, int count)
        {
            int trackindex = IndexOf(t);
            int newindex = trackindex + count;
            if (newindex > Count - 1)
                newindex = 0;
            Move(trackindex, newindex);
            TrackPlaylists.Move(trackindex, newindex);
            RefreshQueuePositionIds();
        }

        public void MoveTrackUp(TrackRepresenter t, int count)
        {
            int trackindex = IndexOf(t);
            int newindex = trackindex - count;
            if (newindex < 0) newindex = Count - 1;
            Move(trackindex, newindex);
            TrackPlaylists.Move(trackindex, newindex);
            RefreshQueuePositionIds();
        }

        public void MoveTrackDown(Track t, int count)
        {
            MoveTrackDown(GetTrackRepresenter(t), count);
        }

        public void MoveTrackUp(Track t, int count)
        {
            MoveTrackUp(GetTrackRepresenter(t), count);
        }

        public void MoveTrackDown(TrackRepresenter t)
        {
            MoveTrackDown(t, 1);
        }

        public void MoveTrackUp(TrackRepresenter t)
        {
            MoveTrackUp(t, 1);
        }

        public void MoveTrackDown(Track t)
        {
            MoveTrackDown(GetTrackRepresenter(t), 1);
        }

        public void MoveTrackUp(Track t)
        {
            MoveTrackUp(GetTrackRepresenter(t));
        }

        #endregion

        public int IndexOf(Track t)
        {
            return IndexOf(GetTrackRepresenter(t));
        }

        public void ClearTracks()
        {
            foreach (var item in this)
                item.Track.QueueID = null;    
            Clear();
            TrackPlaylists.Clear();
            RefreshQueuePositionIds();
            RefreshDuration();
        }

        protected void RefreshDuration()
        {
            Duration = this.Aggregate(TimeSpan.Zero, (current, item) => current + item.Track.DurationTimespan);
        }

        public void RefreshQueuePositionIds()
        {
            for (int i = 0; i < Count; i++)
                this[i].Track.QueueID = (i + 1).ToString();
        }

        public Tuple<Track, IPlaylist> PlayNextTrack()
        {
            if (!HasTracks) return null;
            Track nexttrack = this.First().Track;

            var result = Tuple.Create(nexttrack, TrackPlaylists.First(x => x.Track == nexttrack).Playlist);
            RemoveTrack(nexttrack);
            return result;
        }

        #region Properties
        [XmlIgnore]
        public bool HasTracks
        {
            get
            {
                return Count > 0;
            }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get { return _duration; }
            set { _duration = value; OnPropertyChanged(new PropertyChangedEventArgs("Duration")); }
        }
        
        #endregion
    }

    [Serializable]
    public class TrackRepresenter
    {
        private Track _track;
        [XmlIgnore]
        public Track Track
        {
            get { return _track; }
            set { _track = value; TrackID = value.GenerateHash(); }
        }

        public string TrackID { get; set; }

        public Playlist GetTrack(IEnumerable<Playlist> playlists)
        {
            foreach (var playlist in playlists)
            {
                foreach (var t in playlist.Tracks)
                {
                    if (t.GenerateHash() == TrackID)
                    {
                        Track = t;
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
            Track = t;
        }
    }
}
