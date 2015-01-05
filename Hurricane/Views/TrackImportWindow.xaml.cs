using System.Collections.ObjectModel;
using Hurricane.Music;
using Hurricane.Music.Data;
using MahApps.Metro.Controls;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for TrackImportWindow.xaml
    /// </summary>
    public partial class TrackImportWindow : MetroWindow
    {
        public TrackImportWindow(ObservableCollection<Playlist> playlists, Playlist selectedplaylist, string trackname)
        {
            this.Playlists = playlists;
            this.SelectedPlaylist = selectedplaylist;
            this.Trackname = trackname;
            InitializeComponent();
        }

        public Playlist SelectedPlaylist { get; set; }
        public ObservableCollection<Playlist> Playlists { get; set; }
        public string Trackname { get; set; }
        public bool RememberChoice { get; set; }
        public bool RememberAlsoAfterRestart { get; set; }
    }
}
