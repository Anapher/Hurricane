using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Hurricane.Music.MusicDatabase.EventArgs;

namespace Hurricane.Music.Data
{
    [Serializable]
    public class Playlist : PlaylistBase
    {
        private String _name;
        public override String Name
        {
            get { return _name; }
            set
            {
                SetProperty(value, ref _name);
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
                    AddTrack(t);
                }
            }
            AsyncTrackLoader.Instance.RunAsync(new List<Playlist> {this});
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
                if (!t.TrackExists) RemoveTrack(t);
            }
            OnPropertyChanged("ContainsMissingTracks");
        }

        public override void AddTrack(Track track)
        {
            Tracks.Add(track);
            ShuffleList.Add(track);

            track.IsAdded = true;
            DispatcherTimer tmr = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            tmr.Tick += (s, e) =>
            {
                track.IsAdded = false;
                tmr.Stop();
            };
            tmr.Start();
        }

        public override void RemoveTrack(Track track)
        {
            ShuffleList.Remove(track);
            track.IsRemoving = true;
            DispatcherTimer tmr = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            tmr.Tick += (s, e) =>
            {
                this.Tracks.Remove(track);
                tmr.Stop();
                track.IsRemoving = false; //The track could be also in another playlist
            };
            tmr.Start();
        }

        public override void Clear()
        {
            Tracks.Clear();
            ShuffleList.Clear();
        }

        public override bool CanEdit
        {
            get { return true; }
        }
    }
}
