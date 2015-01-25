using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using GongSolutions.Wpf.DragDrop;
using Hurricane.Music.Playlist;
using Hurricane.Music.Track;
using Hurricane.ViewModels;
using DropTargetInsertionAdorner = Hurricane.DragDrop.DropTargetAdorners.DropTargetInsertionAdorner;

namespace Hurricane.DragDrop
{
    class PlaylistListDropHandler : IDropTarget
    {
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (((dropInfo.Data is PlayableBase || dropInfo.Data is IEnumerable<PlayableBase>) && dropInfo.TargetItem is IPlaylist && dropInfo.DragInfo.SourceCollection != MainViewModel.Instance.MusicManager.FavoritePlaylist.ViewSource))
            {
                dropInfo.DropTargetAdorner = GongSolutions.Wpf.DragDrop.DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
            else if (dropInfo.Data is NormalPlaylist)
            {
                dropInfo.DropTargetAdorner = typeof(DropTargetInsertionAdorner);
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var playlist = (IPlaylist)dropInfo.TargetItem;
            if (dropInfo.Data is PlayableBase)
            {
                var track = (PlayableBase)dropInfo.Data;
                playlist.AddTrack(track);
                ((ObservableCollection<PlayableBase>)((CollectionView)dropInfo.DragInfo.SourceCollection).SourceCollection).Remove(track);
            }
            else if (dropInfo.Data is IEnumerable<PlayableBase>)
            {
                var tracks = (IEnumerable<PlayableBase>)dropInfo.Data;
                foreach (var track in tracks)
                {
                    playlist.AddTrack(track);
                    ((ObservableCollection<PlayableBase>)((CollectionView)dropInfo.DragInfo.SourceCollection).SourceCollection).Remove(track);
                }
            }
            else if (dropInfo.Data is NormalPlaylist)
            {
                var playlistToMove = (NormalPlaylist)dropInfo.Data;
                var collection = (ObservableCollection<NormalPlaylist>)dropInfo.DragInfo.SourceCollection;
                var newIndex = dropInfo.InsertIndex > collection.Count - 1 ? collection.Count - 1 : dropInfo.InsertIndex;
                var currentIndex = collection.IndexOf(playlistToMove);
                if (currentIndex == newIndex) return;
                collection.Move(currentIndex, newIndex);
            }
        }
    }
}
