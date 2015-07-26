using System;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Hurricane.Model.AudioEngine;
using Hurricane.Model.Music.Imagment;
using Hurricane.Model.Music.TrackProperties;

namespace Hurricane.Model.Music.Playable
{
    /// <summary>
    /// The base class for a track which becomes saved
    /// </summary>
    [Serializable]
    public abstract class PlayableBase : PropertyChangedBase, IPlayable
    {
        private string _title;
        private bool _isPlaying;
        private bool _isQueued;
        private DateTime _lastTimePlayed;
        private Artist _artist;
        private Album _album;

        /// <summary>
        /// The title of the track
        /// </summary>
        [XmlIgnore]
        public string Title
        {
            get { return _title; }
            set { SetProperty(value, ref _title); }
        }

        /// <summary>
        /// The <see cref="Artist"/> of the track
        /// </summary>
        [XmlIgnore]
        public Artist Artist
        {
            get { return _artist; }
            set { SetProperty(value, ref _artist); }
        }

        /// <summary>
        /// The album of the track. Null if unknown
        /// </summary>
        [XmlIgnore]
        public Album Album
        {
            get { return _album; }
            set { SetProperty(value, ref _album); }
        }

        /// <summary>
        /// If the track is currently playing
        /// </summary>
        [XmlIgnore]
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { SetProperty(value, ref _isPlaying); }
        }

        /// <summary>
        /// Returns if the track is queued
        /// </summary>
        [XmlIgnore]
        public bool IsQueued
        {
            get { return _isQueued; }
            set { SetProperty(value, ref _isQueued); }
        }

        /// <summary>
        /// The last time this track was played
        /// </summary>
        [XmlIgnore]
        public DateTime LastTimePlayed
        {
            get { return _lastTimePlayed; }
            set { SetProperty(value, ref _lastTimePlayed); }
        }

        /// <summary>
        /// A custom object for some operations
        /// </summary>
        [XmlIgnore]
        public object Tag { get; set; }

        /// <summary>
        /// The id of the track
        /// </summary>
        [XmlIgnore]
        public Guid Guid { get; set; }

        /// <summary>
        /// The duration of the track
        /// </summary>
        [XmlIgnore]
        public TimeSpan Duration { get; set; }

        [XmlIgnore]
        public ImageProvider Cover { get; set; }

        [XmlIgnore]
        public string MusicBrainzId { get; set; }

        string IPlayable.Artist => _artist?.Name;
        public abstract bool IsAvailable { get; }

        public abstract Task<IPlaySource> GetSoundSource();

        public override string ToString()
        {
            return $"{Title} - {Artist?.Name}";
        }
    }
}