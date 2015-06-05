using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Hurricane.Model;
using Hurricane.Model.Music;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Notifications;

namespace Hurricane.ViewModel.MainView
{
    public class PlaylistView : PropertyChangedBase, IViewItem
    {
        public PlaylistView(UserPlaylist playlist)
        {
            Playlist = playlist;
            Icon = new GeometryGroup
            {
                Children =
                    new GeometryCollection
                    {
                        Geometry.Parse(
                            "F1 M 8.000,9.000 L 8.000,15.000 L 15.000,12.000 L 8.000,9.000 Z M 1.000,2.000 L 1.000,3.000 L 21.000,3.000 L 21.000,2.000 L 1.000,2.000 Z M 2.000,0.000 L 2.000,1.000 L 20.000,1.000 L 20.000,0.000 L 2.000,0.000 Z M 0.140,4.000 C 0.063,4.000 0.000,4.058 0.000,4.129 L 0.000,19.871 C 0.000,19.942 0.063,20.000 0.140,20.000 L 21.860,20.000 C 21.937,20.000 22.000,19.942 22.000,19.871 L 22.000,4.129 C 22.000,4.058 21.937,4.000 21.860,4.000 L 0.140,4.000 Z")
                    }
            };
        }

        public ViewCategorie ViewCategorie { get; } = ViewCategorie.Playlist;
        public Geometry Icon { get; }
        public string Text => Playlist.Name;
        public bool IsPlaying { get; set; }
        public UserPlaylist Playlist { get; }

        public async Task Load(MusicDataManager musicDataManager, NotificationManager notificationManager)
        {
            
        }
    }
}