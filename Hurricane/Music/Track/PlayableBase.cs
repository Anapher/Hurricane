using System;
using System.Collections.Generic;
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
using Hurricane.Utilities;
using Hurricane.ViewModelBase;
using Hurricane.ViewModels;

// ReSharper disable ExplicitCallerInfoArgument

namespace Hurricane.Music.Track
{
    [Serializable, XmlType(TypeName = "Playable"),
     XmlInclude(typeof (LocalTrack)),
     XmlInclude(typeof (LocalTrackFragment)),
     XmlInclude(typeof (SoundCloudTrack)),
     XmlInclude(typeof (YouTubeTrack)),
     XmlInclude(typeof (CustomStream))]
    public abstract class PlayableBase : PropertyChangedBase, IEquatable<PlayableBase>, IRepresentable,
        IMusicInformation
    {
        private string _artist;

        private CancellationTokenSource _disposeImageCancellationToken;

        private BitmapImage _image;

        private bool _isAdded;

        private bool _isChecked;

        private bool _isFavorite;

        private bool _isLoadingImage;

        private bool _isOpened;

        private bool _isRemoving;

        private string _queueId;

        private string _title;

        protected PlayableBase()
        {
            AuthenticationCode = DateTime.Now.Ticks;
            IsChecked = true;
        }

        public long AuthenticationCode { get; set; }

        public string Duration { get; set; }
        // ReSharper disable once InconsistentNaming
        [DefaultValue(0)]
        public int kHz { get; set; }

        // ReSharper disable once InconsistentNaming
        public int kbps { get; set; }
        public DateTime TimeAdded { get; set; }
        public DateTime LastTimePlayed { get; set; }

        [DefaultValue(0)]
        public int TrackNumber { get; set; } // number of this track in album; useful for sorting

        [DefaultValue(0.0)]
        public double StartTime { get; set; }

        [DefaultValue(0.0)]
        public double EndTime { get; set; }

        [DefaultValue(true)]
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(value, ref _isChecked); }
        }

        [DefaultValue(false)]
        public bool IsFavorite
        {
            get { return _isFavorite; }
            set
            {
                if (SetProperty(value, ref _isFavorite))
                {
                    if (MainViewModel.Instance.MusicManager == null) return;
                    if (value)
                        // I know that this is ugly as hell but it would be a pain to drop all events to the favorite list. If you got an idea to solve this problem, please tell me
                    {
                        MainViewModel.Instance.MusicManager.FavoritePlaylist.AddTrack(this);
                    }
                    else
                    {
                        MainViewModel.Instance.MusicManager.FavoritePlaylist.RemoveTrack(this);
                    }
                }
            }
        }

        [XmlIgnore]
        public bool IsOpened
        {
            get { return _isOpened; }
            set { SetProperty(value, ref _isOpened); }
        }

        [XmlIgnore]
        public string QueueId
            //I know that the id should be an int, but it wouldn't make sense because what would be the id for non queued track? We would need a converter -> less performance -> string is wurf
        {
            get { return _queueId; }
            set { SetProperty(value, ref _queueId); }
        }

        public TimeSpan DurationTimespan
        {
            get
            {
                return TimeSpan.ParseExact(Duration, Duration.Split(':').Length == 2 ? @"mm\:ss" : @"hh\:mm\:ss", null);
            }
        }

        public string DisplayText
        {
            get
            {
                return !string.IsNullOrEmpty(Artist) && HurricaneSettings.Instance.Config.ShowArtistAndTitle
                    ? string.Format("{0} - {1}", Artist, Title)
                    : Title;
            }
        }

        public abstract bool TrackExists { get; }
        public abstract TrackType TrackType { get; }

        [XmlIgnore]
        public bool IsRemoving
        {
            get { return _isRemoving; }
            set { SetProperty(value, ref _isRemoving); }
        }

        [XmlIgnore]
        public bool IsAdded
        {
            get { return _isAdded; }
            set { SetProperty(value, ref _isAdded); }
        }

        public abstract bool Equals(PlayableBase other);
        public string Album { get; set; }
        public uint Year { get; set; }
        public List<Genre> Genres { get; set; }

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

        TimeSpan IMusicInformation.Duration
        {
            get { return DurationTimespan; }
            set { throw new NotImplementedException(); }
        }

        public async Task<BitmapImage> GetImage()
        {
            if (Image == null)
            {
                var waiter = new AutoResetEvent(false);

                ImageLoadedComplete += (s, e) => { waiter.Set(); };
                Load();
                await Task.Run(() => waiter.WaitOne(2000));
            }
            return Image;
        }

        public event EventHandler ImageLoadedComplete;

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

        [XmlIgnore]
        public bool IsLoadingImage
        {
            get { return _isLoadingImage; }
            set { SetProperty(value, ref _isLoadingImage); }
        }

        public abstract Task<bool> LoadInformation();
        public abstract void OpenTrackLocation();
        public abstract Task<IWaveSource> GetSoundSource();
        protected abstract Task LoadImage(DirectoryInfo albumCoverDirectory);

        public async void Load()
        {
            if (_disposeImageCancellationToken != null) _disposeImageCancellationToken.Cancel();
            if (Image == null)
            {
                IsLoadingImage = true;
                var albumCoverDirectory = new DirectoryInfo(HurricaneSettings.Paths.CoverDirectory);
                Image = MusicCoverManager.GetTrackImage(this, albumCoverDirectory);
                if (Image == null) await LoadImage(albumCoverDirectory);
                IsLoadingImage = false;
            }

            OnImageLoadComplete();
        }

        public virtual async void Unload()
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

        public override string ToString()
        {
            return DisplayText;
        }

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
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (StartTime == 0 && EndTime == 0)
                return source;

            var startTime = TimeSpan.FromMilliseconds(StartTime);
            var endTime = TimeSpan.FromMilliseconds(EndTime);
            return source.AppendSource(x => new CutSource(x, startTime, endTime - startTime));
        }

        public virtual async Task<bool> CheckTrack()
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

        public static Genre StringToGenre(string genre)
        {
            switch (genre)
            {
                case "Country":
                    return Genre.Country;

                case "Reggae":
                    return Genre.Reggae;

                case "Alternative":
                    return Genre.Alternative;

                case "Vocal":
                    return Genre.AcousticAndVocal;

                case "Trance":
                    return Genre.Trance;

                case "Classical":
                case "Classic":
                    return Genre.Classical;

                case "Game":
                case "Sound Clip":
                case "Soundtrack":
                    return Genre.SoundTrack;

                case "Bass":
                case "Drum & Bass":
                    return Genre.DrumAndBass;

                case "Ethnic":
                    return Genre.Ethnic;

                case "Darkwave":
                case "Gothic":
                case "Grunge":
                case "Metal":
                case "Polsk Punk":
                case "Acid Punk":
                case "Death Metal":
                case "Heavy Metal":
                case "Black Metal":
                case "Thrash Metal":
                    return Genre.Metal;

                case "Techno-Industrial":
                case "Electronic":
                case "Euro-Techno":
                case "Techno":
                case "Disco":
                case "Electropop":
                case "Electropop & Disco":
                    return Genre.ElectropopAndDisco;

                case "Eurodance":
                case "Dance":
                case "House":
                case "Club":
                case "Dance Hall":
                case "Club-House":
                case "Dance & House":
                    return Genre.DanceAndHouse;

                case "Christian Rap":
                case "Trip-Hop":
                case "Rap":
                case "Hip-Hop":
                case "Freestyle":
                case "Rap & Hip-Hop":
                    return Genre.RapAndHipHop;

                case "Pop/Funk":
                case "Pop-Folk":
                case "Pop":
                    return Genre.Pop;

                case "Trailer":
                case "Symphony":
                case "Dream":
                case "Instrumental Rock":
                case "Instrumental Pop":
                case "Meditative":
                case "Instrumental":
                    return Genre.Instrumental;

                case "Swing":
                case "Acid Jazz":
                case "Soul":
                case "Jazz+Funk":
                case "R&B":
                case "Oldies":
                case "Jazz":
                case "Funk":
                case "Blues":
                case "Rhythmic Soul":
                    return Genre.JazzAndBlues;

                case "Gothic Rock":
                case "Progressive Rock":
                case "Psychedelic Rock":
                case "Symphonic Rock":
                case "Slow Rock":
                case "Rock & Roll":
                case "Hard Rock":
                case "Classic Rock":
                case "Southern Rock":
                case "Punk":
                case "Alternative Rock":
                case "Rock":
                case "Punk Rock":
                    return Genre.Rock;

                case "Indie":
                case "Indie Pop":
                    return Genre.IndiePop;

                default:
                    return Genre.Other;
            }
        }

        public static string GenreToString(Genre genre)
        {
            return genre.ToString().ToSentenceCase().Replace("And", "&");
        }
    }

    public enum TrackType
    {
        File,
        Stream
    }
}