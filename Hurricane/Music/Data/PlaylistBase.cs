using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Serialization;
using Hurricane.ViewModelBase;

namespace Hurricane.Music.Data
{
    public abstract class PlaylistBase : PropertyChangedBase, IPlaylist
    {
        protected Random _Random;

        protected PlaylistBase()
        {
            _tracks = new ObservableCollection<Track>();
            _Random = new Random();
        }

        protected ObservableCollection<Track> _tracks;
        public ObservableCollection<Track> Tracks
        {
            get { return _tracks; }
        }

        private ICollectionView _viewsource;
        [XmlIgnore]
        public ICollectionView ViewSource
        {
            get
            {
                return _viewsource;
            }
            set
            {
                SetProperty(value, ref _viewsource);
            }
        }

        private string _searchtext;
        [XmlIgnore]
        public string SearchText
        {
            get { return _searchtext; }
            set
            {
                if (SetProperty(value, ref _searchtext))
                    ViewSource.Refresh();
            }
        }

        public virtual void LoadList()
        {
            if (Tracks != null)
            {
                Tracks.CollectionChanged += async (s, e) =>
                {
                    if (e.Action != NotifyCollectionChangedAction.Move || e.NewItems == null || e.NewItems.Count <= 0)
                        return;
                    var track = e.NewItems[0] as Track;
                    if (track == null) return;
                    track.IsAdded = true;
                    await Task.Delay(500);
                    track.IsAdded = false;
                };
                ViewSource = CollectionViewSource.GetDefaultView(Tracks);
                ViewSource.Filter = (item) => string.IsNullOrWhiteSpace(SearchText) || item.ToString().ToUpper().Contains(SearchText.ToUpper());
                ShuffleList = new List<Track>(Tracks);
            }
        }

        public abstract void Clear();

        #region Shuffle

        [XmlIgnore]
        public List<Track> ShuffleList { get; set; }

        private bool _addedFavoriteTracksTwoTimes;
        protected void CreateShuffleList()
        {
            this.ShuffleList = new List<Track>(this.Tracks);
            if (Settings.HurricaneSettings.Instance.Config.ShufflePreferFavoritTracks)
            {
                ShuffleList.AddRange(this.Tracks.Where(x => x.IsFavorite));
                _addedFavoriteTracksTwoTimes = true;
            }
            else
            {
                _addedFavoriteTracksTwoTimes = false;
            }
        }

        protected void RemoveFromShuffleList(Track track)
        {
            ShuffleList.Remove(track);
            if (_addedFavoriteTracksTwoTimes) ShuffleList.Remove(track);
        }

        public Track GetRandomTrack(Track currentTrack)
        {
            if (Tracks.Count == 0) return null;

            if (ShuffleList.Count == 0) CreateShuffleList();
            bool hasrefreshed = false;
            while (true)
            {
                int i = _Random.Next(0, ShuffleList.Count);
                var track = ShuffleList[i];

                if (track != currentTrack && track.TrackExists)
                {
                    RemoveFromShuffleList(track);
                    return track;
                }
                RemoveFromShuffleList(track);
                if (ShuffleList.Count == 0)
                {
                    if (hasrefreshed)
                        return null;
                    CreateShuffleList();
                    hasrefreshed = true;
                }
            }
        }

        #endregion

        public abstract void AddTrack(Track track);

        public abstract void RemoveTrack(Track track);

        public abstract string Name { get; set; }

        public async Task<int> RemoveDuplicates()
        {
            int counter = this.Tracks.Count;
            List<Track> noduplicates = null;
            await Task.Run(() => noduplicates = Tracks.Distinct(new TrackComparer()).ToList());

            if (noduplicates.Count > 0 && noduplicates.Count != this.Tracks.Count)
            {
                var duplicateList = this.Tracks.ToList();
                foreach (var noduplicate in noduplicates)
                {
                    duplicateList.Remove(noduplicate);
                }

                foreach (var track in duplicateList)
                {
                    this.RemoveTrack(track);
                }
            }

            return counter - noduplicates.Count;
        }

        public abstract bool CanEdit { get; }


        public bool ContainsMissingTracks
        {
            get { return Tracks.Any(t => !t.TrackExists); }
        }

        public void RemoveMissingTracks()
        {
            for (int i = Tracks.Count - 1; i > -1; i--)
            {
                Track t = Tracks[i];
                if (!t.TrackExists) RemoveTrack(t);
            }
            OnPropertyChanged("ContainsMissingTracks");
        }
    }
}
