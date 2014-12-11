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
            Tracks = new List<Track>();
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

        public List<Track> Tracks { get; protected set; }

        [XmlIgnore]
        public DataVirtualization.VirtualizingCollection<Track> TrackCollection { get; set; }

        private CollectionView viewsource;
        [XmlIgnore]
        public CollectionView ViewSource
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

        public void RefreshList(Predicate<object> filter)
        {
            if (Tracks != null)
            {
                DataVirtualization.TrackProvider loader = new DataVirtualization.TrackProvider(Tracks);
                TrackCollection = new DataVirtualization.VirtualizingCollection<Track>(loader, 200);
                ViewSource = (CollectionView)CollectionViewSource.GetDefaultView(TrackCollection);
                ViewSource.Filter = filter;
            }
        }

        public void AddFiles(EventHandler<TrackImportProgressChangedEventArgs> progresschanged, bool FromAnotherThread, params string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                FileInfo fi = new FileInfo(paths[i]);
                if (fi.Exists)
                {
                    if (progresschanged != null) progresschanged(this, new TrackImportProgressChangedEventArgs(i, paths.Length, fi.Name));
                    Track t = new Track();
                    t.Path = fi.FullName;
                    if (!t.LoadInformations()) continue;
                    t.TimeAdded = DateTime.Now;
                    if (FromAnotherThread) { System.Windows.Application.Current.Dispatcher.Invoke(() => this.TrackCollection.Add(t)); } else {this.TrackCollection.Add(t); }
                }
            }

            //if(FromAnotherThread) {System.Windows.Application.Current.Dispatcher.Invoke(() =>this.ViewSource.Refresh()); } else {this.ViewSource.SourceCollection}
            
        }

        public void AddFiles(bool FromAnotherThread, params string[] paths)
        {
            this.AddFiles(null,FromAnotherThread, paths);
        }

        public void ReloadTrackInformations(EventHandler<TrackImportProgressChangedEventArgs> progresschanged, bool FromAnotherThread)
        {
            foreach (Track t in this.Tracks)
            {
                if (progresschanged != null) progresschanged(this, new TrackImportProgressChangedEventArgs(this.Tracks.IndexOf(t), Tracks.Count, t.ToString()));
                if (t.TrackExists)
                {
                    t.LoadInformations();
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
            for (int i = Tracks.Count -1; i > -1; i--)
            {
                Track t = Tracks[i];
                if (!t.TrackExists) this.TrackCollection.Remove(t);
            }
            OnPropertyChanged("ContainsMissingTracks");
        }

        /// <summary>
        /// Removes all duplicated tracks
        /// </summary>
        /// <returns>Returns the number of the removed tracks</returns>
        public int RemoveDuplicates(bool FromAnotherThread = false)
        {
            int counter = this.Tracks.Count;
            IEnumerable<Track> noduplicates = this.Tracks.Distinct(new TrackComparer());
            if (noduplicates.Any() && noduplicates.Count() != this.Tracks.Count)
            {
                if (FromAnotherThread) { System.Windows.Application.Current.Dispatcher.Invoke(() => { this.Tracks = new List<Track>(noduplicates); RefreshList(ViewSource.Filter); }); } else { this.Tracks = new List<Track>(noduplicates); RefreshList(ViewSource.Filter); };
            }
            return counter - noduplicates.Count();
        }
    }
}
