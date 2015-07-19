using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml;
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
        private Album _album;
        private bool _isPlaying;
        private DateTime _lastTimePlayed;
        private Artist _artist;

        /// <summary>
        /// The title of the track
        /// </summary>
        [XmlAttribute]
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
            set
            {
                if (SetProperty(value, ref _artist) && value != null)
                    ArtistGuid = value.Guid;
            }
        }

        /// <summary>
        /// The album of the track. Null if unkown
        /// </summary>
        [XmlIgnore]
        public Album Album
        {
            get { return _album; }
            set
            {
                if (SetProperty(value, ref _album) && value != null)
                    AlbumGuid = value.Guid;
            }
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
        /// The last time this track was played
        /// </summary>
        [XmlAttribute]
        public DateTime LastTimePlayed
        {
            get { return _lastTimePlayed; }
            set { SetProperty(value, ref _lastTimePlayed); }
        }

        [Browsable(false)]
        [XmlAttribute]
        public Guid ArtistGuid { get; set; }

        [Browsable(false)]
        [XmlAttribute]
        public Guid AlbumGuid { get; set; }

        [XmlAttribute]
        public string MusicBrainzId { get; set; }

        string IPlayable.Artist => _artist?.Name;

        /// <summary>
        /// The duration of the track
        /// </summary>
        [XmlIgnore]
        public TimeSpan Duration { get; set; }

        // XmlSerializer does not support TimeSpan, so use this property for serialization instead.
        [Browsable(false)]
        [XmlElement(DataType = "duration", ElementName = "Duration")]
        public string DurationString
        {
            get
            {
                return XmlConvert.ToString(Duration);   
            }
            set
            {
                Duration = string.IsNullOrEmpty(value)
                    ? TimeSpan.Zero
                    : XmlConvert.ToTimeSpan(value);
            }
        }

        public ImageProvider Cover { get; set; }
        public abstract bool IsAvailable { get; }

        public abstract Task<IPlaySource> GetSoundSource();
        public abstract Task LoadImage();

        public override string ToString()
        {
            return $"{Title} - {Artist?.Name}";
        }
    }
}