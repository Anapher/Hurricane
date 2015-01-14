using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Hurricane.Music.Data;
using Hurricane.Music.Track;

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

        public void AddTrack(PlayableBase track, IPlaylist playlist)
        {
            Add(new TrackRepresenter(track));
            TrackPlaylists.Add(new TrackPlaylistPair(track, playlist));
            track.QueueID = (IndexOf(GetTrackRepresenter(track)) + 1).ToString();
            RefreshDuration();
        }

        protected TrackRepresenter GetTrackRepresenter(PlayableBase t)
        {
            return this.FirstOrDefault(x => x.Track == t);
        }

        public void RemoveTrack(PlayableBase track)
        {
            var trackrepresentertoremove = GetTrackRepresenter(track);
            if (trackrepresentertoremove == null) return;
            Remove(trackrepresentertoremove);
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

        public void MoveTrackDown(PlayableBase t, int count)
        {
            MoveTrackDown(GetTrackRepresenter(t), count);
        }

        public void MoveTrackUp(PlayableBase t, int count)
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

        public void MoveTrackDown(PlayableBase t)
        {
            MoveTrackDown(GetTrackRepresenter(t), 1);
        }

        public void MoveTrackUp(PlayableBase t)
        {
            MoveTrackUp(GetTrackRepresenter(t));
        }

        #endregion

        public int IndexOf(PlayableBase t)
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

        public Tuple<PlayableBase, IPlaylist> PlayNextTrack()
        {
            if (!HasTracks) return null;
            PlayableBase nexttrack = this.First().Track;

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
        private PlayableBase _track;
        [XmlIgnore]
        public PlayableBase Track
        {
            get { return _track; }
            set { _track = value; }
        }

        public long TrackID { get; set; }

        public Playlist GetTrack(IEnumerable<Playlist> playlists)
        {
            foreach (var playlist in playlists)
            {
                foreach (var t in playlist.Tracks)
                {
                    if (t.AuthenticationCode == TrackID)
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

        public TrackRepresenter(PlayableBase t)
        {
            Track = t;
        }
    }
}
