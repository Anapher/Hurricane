using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using CSCore;
using Hurricane.Music.Data;
using Hurricane.Settings;
using Hurricane.ViewModelBase;

namespace Hurricane.Music.Track
{
    [Serializable, XmlInclude(typeof(LocalTrack)), XmlInclude(typeof(SoundCloudTrack)), XmlInclude(typeof(YouTubeTrack)), XmlType(TypeName = "Playable")]
    public abstract class PlayableBase : PropertyChangedBase, IEquatable<PlayableBase>, IRepresentable, IMusicInformation
    {
        #region Events
        public event EventHandler ImageLoadComplete;

        #endregion

        public long AuthenticationCode { get; set; }
        
        public string Duration { get; set; }
        public int kHz { get; set; }
        public int kbps { get; set; }
        public DateTime TimeAdded { get; set; }
        public DateTime LastTimePlayed { get; set; }
        public string Album { get; set; }
        public uint Year { get; set; }
        public string Genres { get; set; }

        private bool _isfavorite;
        public bool IsFavorite
        {
            get { return _isfavorite; }
            set
            {
                if (SetProperty(value, ref _isfavorite))
                {
                    if (ViewModels.MainViewModel.Instance.MusicManager == null) return;
                    if (value) // I know that this is ugly as hell but it would be a pain to drop all events to the favorite list. If you got an idea to solve this problem, please tell me
                    {
                        ViewModels.MainViewModel.Instance.MusicManager.FavoritePlaylist.AddTrack(this);
                    }
                    else { ViewModels.MainViewModel.Instance.MusicManager.FavoritePlaylist.RemoveTrack(this); }
                }
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged("DisplayText"); }
        }

        private string _artist;
        public string Artist
        {
            get { return _artist; }
            set { _artist = value; OnPropertyChanged("DisplayText"); }
        }

        private bool _isOpened;
        [XmlIgnore]
        public bool IsOpened
        {
            get { return _isOpened; }
            set
            {
                SetProperty(value, ref _isOpened);
            }
        }

        private BitmapImage _image;
        [XmlIgnore]
        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                if (value != null && !value.IsFrozen) value.Freeze(); //The image has to be thread save
                SetProperty(value, ref _image);
            }
        }

        private bool _isloadingimage;
        [XmlIgnore]
        public bool IsLoadingImage
        {
            get { return _isloadingimage; }
            set
            {
                SetProperty(value, ref _isloadingimage);
            }
        }

        private String _queueid;
        [XmlIgnore]
        public String QueueID //I know that the id should be an int, but it wouldn't make sense because what would be the id for non queued track? We would need a converter -> less performance -> string is wurf
        {
            get { return _queueid; }
            set
            {
                SetProperty(value, ref _queueid);
            }
        }

        #region ReadOnly Properties

        public TimeSpan DurationTimespan
        {
            get
            {
                return TimeSpan.ParseExact(Duration, Duration.Split(':').Length == 2 ? @"mm\:ss" : @"hh\:mm\:ss", null);
            }
        }

        public string DisplayText
        {
            get { return !string.IsNullOrEmpty(Artist) && HurricaneSettings.Instance.Config.ShowArtistAndTitle ? string.Format("{0} - {1}", Artist, Title) : Title; }
        }

        #endregion

        #region Abstract Properties

        public abstract bool TrackExists { get; }
        public abstract Task<bool> LoadInformation();
        public abstract void Load();
        public abstract void OpenTrackLocation();
        public abstract TrackType TrackType { get; }
        public abstract Task<IWaveSource> GetSoundSource();
        public abstract bool Equals(PlayableBase other);

        public virtual void Unload()
        {
            if (Image != null)
            {
                if (Image.StreamSource != null) Image.StreamSource.Dispose();
                Image = null;
            }
        }

        public virtual void RefreshTrackExists()
        {
            OnPropertyChanged("TrackExists");
        }

        #endregion

        public override string ToString()
        {
            return DisplayText;
        }

        #region Animations

        private bool _isremoving;
        [XmlIgnore]
        public bool IsRemoving
        {
            get { return _isremoving; }
            set
            {
                SetProperty(value, ref _isremoving);
            }
        }

        private bool _isadded;
        [XmlIgnore]
        public bool IsAdded
        {
            get { return _isadded; }
            set
            {
                SetProperty(value, ref _isadded);
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void OnImageLoadComplete()
        {
            var handler = ImageLoadComplete;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected void SetDuration(TimeSpan timeSpan)
        {
            Duration = timeSpan.ToString(timeSpan.Hours == 0 ? @"mm\:ss" : @"hh\:mm\:ss");
        }

        #endregion

        protected PlayableBase()
        {
            AuthenticationCode = DateTime.Now.Ticks;
        }

        TimeSpan IMusicInformation.Duration
        {
            get { return DurationTimespan; }
            set
            {
                throw new NotImplementedException();
            }
        }

        public async Task<BitmapImage> GetImage()
        {
            if (Image == null)
            {
                using (var waiter = new AutoResetEvent(false))
                {
                    ImageLoadComplete += (s, e) => { waiter.Set(); };
                    Load();
                    await Task.Run(() => waiter.WaitOne(2000));
                }
            }
            return Image;
        }
    }

    public enum TrackType { File, Stream }
}