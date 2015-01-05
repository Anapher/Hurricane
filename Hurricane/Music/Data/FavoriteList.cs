using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hurricane.Music.Data
{
   public class FavoriteList : PlaylistBase
    {
       public void Initalize(IEnumerable<Playlist> playlists)
       {
           foreach (var playlist in playlists)
           {
               foreach (var track in playlist.Tracks.Where(track => track.IsFavorite))
                   this.Tracks.Add(track);
           }
       }

       public override void AddTrack(Track track)
       {
           track.IsFavorite = true;
           this.Tracks.Add(track);
           this.ShuffleList.Add(track);
       }

       public override void RemoveTrack(Track track)
       {
           track.IsFavorite = false;
           this.Tracks.Remove(track);
           RemoveFromShuffleList(track);
       }

       public override string Name
       {
           get { return Application.Current.Resources["Favorites"].ToString(); }
           set { throw new Exception(); }
       }

       public override void Clear()
       {
           foreach (var track in this.Tracks)
           {
               track.IsFavorite = false;
           }
       }

       public override bool CanEdit
       {
           get { return false; }
       }
    }
}
