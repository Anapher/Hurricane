using System.Collections.ObjectModel;
using Hurricane.Music.Playlist;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for TrackImportWindow.xaml
    /// </summary>
    public partial class TrackImportWindow
    {
        public TrackImportWindow(ObservableCollection<NormalPlaylist> playlists, NormalPlaylist selectedplaylist, string trackname)
        {
            Playlists = playlists;
            SelectedPlaylist = selectedplaylist;
            Trackname = trackname;
            InitializeComponent();
        }

        public NormalPlaylist SelectedPlaylist { get; set; }
        public ObservableCollection<NormalPlaylist> Playlists { get; set; }
        public string Trackname { get; set; }
        public bool RememberChoice { get; set; }
        public bool RememberAlsoAfterRestart { get; set; }
    }
}
