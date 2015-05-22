using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;

namespace Hurricane.Model.Music
{
    public class MusicDataManager
    {
        public MusicDataManager()
        {
            Tracks = new ObservableCollection<PlayableBase>();
            Playlists = new ObservableCollection<UserPlaylist>();
        }

        public ObservableCollection<PlayableBase> Tracks { get; set; }
        public ObservableCollection<UserPlaylist> Playlists { get; set; }
        public MusicManager MusicManager { get; set; }
    }
}