using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Hurricane.Music.Track;

namespace Hurricane.Music.Playlist
{
   public class FavoriteList : PlaylistBase
    {
       public void Initalize(IEnumerable<NormalPlaylist> playlists)
       {
           foreach (var playlist in playlists)
           {
               foreach (var track in playlist.Tracks.Where(track => track.IsFavorite))
                   Tracks.Add(track);
           }
       }

       public override void AddTrack(PlayableBase track)
       {
           track.IsFavorite = true;
           Tracks.Add(track);
           ShuffleList.Add(track);
       }

       public override void RemoveTrack(PlayableBase track)
       {
           track.IsFavorite = false;
           Tracks.Remove(track);
           RemoveFromShuffleList(track);
       }

       public override string Name
       {
           get { return Application.Current.Resources["Favorites"].ToString(); }
           set { throw new Exception(); }
       }

       public override void Clear()
       {
           foreach (var track in Tracks)
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
