using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.IO;
using Hurricane.Utilities;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Hurricane.Music
{
    [Serializable]
    public class Playlist : ViewModelBase.PropertyChangedBase
    {
        public Playlist()
        {
            Tracks = new ObservableCollection<Track>();
        }

        private String name;
        public String Name
        {
            get { return name; }
            set
            {
                SetProperty(value, ref name);
            }
        }

        public ObservableCollection<Track> Tracks { get; protected set; }

        private string searchtext;
        [XmlIgnore]
        public string SearchText
        {
            get { return searchtext; }
            set
            {
                if (SetProperty(value, ref searchtext))
                    ViewSource.Refresh();
            }
        }

        private ICollectionView viewsource;
        [XmlIgnore]
        public ICollectionView ViewSource
        {
            get
            {
                return viewsource;
            }
            set
            {
                SetProperty(value, ref viewsource);
            }
        }

        public void LoadList()
        {
            if (Tracks != null)
            {
                ViewSource = CollectionViewSource.GetDefaultView(Tracks);
                ViewSource.Filter = (item) =>
                {
                    if (string.IsNullOrWhiteSpace(SearchText)) { return true; } else { return item.ToString().ToUpper().Contains(SearchText.ToUpper()); }
                };
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
                    Track t = new Track();
                    t.Path = fi.FullName;
                    if (!await t.LoadInformations()) continue;
                    t.TimeAdded = DateTime.Now;
                    this.AddTrackWithAnimation(t);
                }
            }
        }

        public async Task AddFiles(params string[] paths)
        {
            await this.AddFiles(null, paths);
        }

        public async Task ReloadTrackInformations(EventHandler<TrackImportProgressChangedEventArgs> progresschanged, bool FromAnotherThread)
        {
            foreach (Track t in this.Tracks)
            {
                if (progresschanged != null) progresschanged(this, new TrackImportProgressChangedEventArgs(this.Tracks.IndexOf(t), Tracks.Count, t.ToString()));
                if (t.TrackExists)
                {
                    await t.LoadInformations();
                }
            }
        }

        public bool ContainsMissingTracks
        {
            get
            {
                foreach (Track t in this.Tracks)
                {
                    if (!t.TrackExists) return true;
                }
                return false;
            }
        }

        public void RemoveMissingTracks()
        {
            for (int i = Tracks.Count - 1; i > -1; i--)
            {
                Track t = Tracks[i];
                if (!t.TrackExists) this.RemoveTrackWithAnimation(t);
            }
            OnPropertyChanged("ContainsMissingTracks");
        }

        public void RemoveTrackWithAnimation(Track track)
        {
            track.IsRemoving = true;
            System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer();
            tmr.Interval = TimeSpan.FromMilliseconds(500);
            tmr.Tick += (s, e) => { this.Tracks.Remove(track); tmr.Stop(); };
            tmr.Start();
        }

        public void AddTrackWithAnimation(Track track)
        {
            track.IsAdded = true;
            this.Tracks.Add(track);
            System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer();
            tmr.Interval = TimeSpan.FromMilliseconds(500);
            tmr.Tick += (s, e) => { track.IsAdded = false; tmr.Stop(); };
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
            await Task.Run(() => noduplicates = this.Tracks.Distinct(new TrackComparer()));
            if (noduplicates.Any() && noduplicates.Count() != this.Tracks.Count)
            {
                this.Tracks = new ObservableCollection<Track>(noduplicates);
                LoadList();
            }
            return counter - noduplicates.Count();
        }
    }
}
