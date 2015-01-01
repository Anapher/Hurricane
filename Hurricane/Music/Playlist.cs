using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml.Serialization;
using Hurricane.Music.MusicDatabase.EventArgs;
using Hurricane.ViewModelBase;

namespace Hurricane.Music
{
    [Serializable]
    public class Playlist : PropertyChangedBase
    {
        public Playlist()
        {
            Tracks = new ObservableCollection<Track>();
        }

        private String _name;
        public String Name
        {
            get { return _name; }
            set
            {
                SetProperty(value, ref _name);
            }
        }

        public ObservableCollection<Track> Tracks { get; protected set; }

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

        [XmlIgnore]
        public List<Track> ShuffleList { get; set; }

        public void LoadList()
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

        public async Task AddFiles(EventHandler<TrackImportProgressChangedEventArgs> progresschanged, params string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                FileInfo fi = new FileInfo(paths[i]);
                if (fi.Exists)
                {
                    if (progresschanged != null) progresschanged(this, new TrackImportProgressChangedEventArgs(i, paths.Length, fi.Name));
                    Track t = new Track { Path = fi.FullName };
                    if (!await t.LoadInformation()) continue;
                    t.TimeAdded = DateTime.Now;
                    AddTrackWithAnimation(t);
                }
            }
        }

        public async Task AddFiles(params string[] paths)
        {
            await AddFiles(null, paths);
        }

        public async Task ReloadTrackInformation(EventHandler<TrackImportProgressChangedEventArgs> progresschanged)
        {
            foreach (Track t in Tracks)
            {
                if (progresschanged != null) progresschanged(this, new TrackImportProgressChangedEventArgs(Tracks.IndexOf(t), Tracks.Count, t.ToString()));
                if (t.TrackExists)
                {
                    await t.LoadInformation();
                }
            }
        }

        public bool ContainsMissingTracks
        {
            get { return Tracks.Any(t => !t.TrackExists); }
        }

        public void RemoveMissingTracks()
        {
            for (int i = Tracks.Count - 1; i > -1; i--)
            {
                Track t = Tracks[i];
                if (!t.TrackExists) RemoveTrackWithAnimation(t);
            }
            OnPropertyChanged("ContainsMissingTracks");
        }

        public void RemoveTrackWithAnimation(Track track)
        {
            ShuffleList.Remove(track);
            track.IsRemoving = true;
            DispatcherTimer tmr = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            tmr.Tick += (s, e) => { this.Tracks.Remove(track); tmr.Stop(); };
            tmr.Start();
        }

        public void AddTrackWithAnimation(Track track)
        {
            ShuffleList.Add(track);
            track.IsAdded = true;
            Tracks.Add(track);
            DispatcherTimer tmr = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            tmr.Tick += (s, e) =>
            {
                track.IsAdded = false;
                tmr.Stop();
            };
            tmr.Start();
        }

        /// <summary>
        /// Removes all duplicated tracks
        /// </summary>
        /// <returns>Returns the number of the removed tracks</returns>
        public async Task<int> RemoveDuplicates()
        {
            int counter = this.Tracks.Count;
            IEnumerable<Track> noduplicates = null;
            await Task.Run(() => noduplicates = Tracks.Distinct(new TrackComparer()));
            if (noduplicates.Any() && noduplicates.Count() != this.Tracks.Count)
            {
                this.Tracks = new ObservableCollection<Track>(noduplicates);
                LoadList();
            }
            return counter - noduplicates.Count();
        }
    }
}
