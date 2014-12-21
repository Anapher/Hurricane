using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaktionslogik für TrackImportWindow.xaml
    /// </summary>
    public partial class TrackImportWindow : MahApps.Metro.Controls.MetroWindow
    {
        public TrackImportWindow(ObservableCollection<Music.Playlist> playlists, Music.Playlist selectedplaylist, string trackname)
        {
            this.Playlists = playlists;
            this.SelectedPlaylist = selectedplaylist;
            this.Trackname = trackname;
            InitializeComponent();
        }

        public Music.Playlist SelectedPlaylist { get; set; }
        public ObservableCollection<Music.Playlist> Playlists { get; set; }
        public string Trackname { get; set; }
        public bool RememberChoice { get; set; }
        public bool RememberAlsoAfterRestart { get; set; }
    }
}
