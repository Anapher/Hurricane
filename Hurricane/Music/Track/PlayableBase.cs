using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using CSCore;
using Hurricane.Music.AudioEngine;
using Hurricane.Music.Data;
using Hurricane.Music.MusicCover;
using Hurricane.Settings;
using Hurricane.ViewModelBase;

namespace Hurricane.Music.Track
{
    [Serializable, XmlInclude(typeof(LocalTrack)), XmlInclude(typeof(LocalTrackFragment)), XmlInclude(typeof(SoundCloudTrack)), XmlInclude(typeof(YouTubeTrack)), XmlType(TypeName = "Playable")]
    public abstract class PlayableBase : PropertyChangedBase, IEquatable<PlayableBase>, IRepresentable, IMusicInformation
    {
        #region Events
        public event EventHandler ImageLoadedComplete;

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

        [DefaultValue(0)]
        public int TrackNumber { get; set; }    // number of this track in album; useful for sorting

        [DefaultValue(0.0)]
        public double StartTime { get; set; }

        [DefaultValue(0.0)]
        public double EndTime { get; set; }

        private bool _isChecked;
        [DefaultValue(true)]
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                SetProperty(value, ref _isChecked);
            }
        }

        private bool _isfavorite;
        [DefaultValue(false)]
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
            set
            {
                _title = value;
                OnPropertyChanged("DisplayText");
                OnPropertyChanged();
            }
        }

        private string _artist;
        public string Artist
        {
            get { return _artist; }
            set
            {
                _artist = value;
                OnPropertyChanged("DisplayText");
                OnPropertyChanged();
            }
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

        #region Abstract members

        public abstract bool TrackExists { get; }
        public abstract Task<bool> LoadInformation();
        public abstract void OpenTrackLocation();
        public abstract TrackType TrackType { get; }
        public abstract Task<IWaveSource> GetSoundSource();
        public abstract bool Equals(PlayableBase other);
        protected abstract Task LoadImage(DirectoryInfo albumCoverDirectory);

        #endregion

        #region Image

        public async void Load()
        {
            if (_disposeImageCancellationToken != null) _disposeImageCancellationToken.Cancel();
            if (Image == null)
            {
                IsLoadingImage = true;
                var albumCoverDirectory = new DirectoryInfo(HurricaneSettings.Instance.CoverDirectory);
                Image = MusicCoverManager.GetTrackImage(this, albumCoverDirectory);
                if (Image == null) await LoadImage(albumCoverDirectory);
                IsLoadingImage = false;
            }

            OnImageLoadComplete();
        }

        private CancellationTokenSource _disposeImageCancellationToken;
        public async virtual void Unload()
        {
            if (Image != null)
            {
                _disposeImageCancellationToken = new CancellationTokenSource();
                try
                {
                    await Task.Delay(2000, _disposeImageCancellationToken.Token); //Some animations need that
                }
                catch (TaskCanceledException)
                {
                    return;
                }

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
            var handler = ImageLoadedComplete;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected void SetDuration(TimeSpan timeSpan)
        {
            Duration = timeSpan.ToString(timeSpan.Hours == 0 ? @"mm\:ss" : @"hh\:mm\:ss");
        }

        protected IWaveSource CutWaveSource(IWaveSource source)
        {
            if (StartTime == 0 && EndTime == 0)
                return source;

            var startTime = TimeSpan.FromMilliseconds(StartTime);
            var endTime = TimeSpan.FromMilliseconds(EndTime);
            return source.AppendSource(x => new CutSource(x, startTime, endTime - startTime));
        }

        #endregion

        #region Public Methods

        public async Task<bool> CheckTrack()
        {
            if (!TrackExists) return false;
            try
            {
                using (var soundSource = await GetSoundSource())
                {
                    SetDuration(soundSource.GetLength());
                    kHz = soundSource.WaveFormat.SampleRate/1000;
                }
            }
            catch (Exception)
            {
                return false;
            }

            IsChecked = true;
            return true;
        }

        #endregion

        protected PlayableBase()
        {
            AuthenticationCode = DateTime.Now.Ticks;
            IsChecked = true;
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
                    ImageLoadedComplete += (s, e) => { waiter.Set(); };
                    Load();
                    await Task.Run(() => waiter.WaitOne(2000));
                }
            }
            return Image;
        }
    }

    public enum TrackType { File, Stream }
}