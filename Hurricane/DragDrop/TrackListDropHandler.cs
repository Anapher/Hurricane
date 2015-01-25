using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using Hurricane.Music.Track;
using DropTargetInsertionAdorner = Hurricane.DragDrop.DropTargetAdorners.DropTargetInsertionAdorner;

namespace Hurricane.DragDrop
{
    public class TrackListDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.Effects = DragDropEffects.Move;
            dropInfo.DropTargetAdorner = typeof(DropTargetInsertionAdorner);
        }

        public void Drop(IDropInfo dropInfo)
        {
            var collection = (ObservableCollection<PlayableBase>)((ICollectionView)dropInfo.TargetCollection).SourceCollection;

            if (dropInfo.Data is PlayableBase)
            {
                var track = (PlayableBase)dropInfo.Data;
                var index = dropInfo.InsertIndex >= collection.Count
                    ? collection.Count - 1
                    : dropInfo.InsertIndex;

                var newIndex = collection.IndexOf(track);
                if (newIndex == index) return;
                collection.Move(newIndex, index);
            }
            else if (dropInfo.Data is IEnumerable<PlayableBase>)
            {
                var tracks = ((IEnumerable<PlayableBase>)dropInfo.Data).OrderBy(x => collection.IndexOf(x)).ToList();

                int index;
                if (dropInfo.InsertIndex >= collection.Count)
                {
                    index = collection.Count - 1;
                }
                else
                {
                    index = dropInfo.InsertIndex;
                    if (collection.IndexOf(tracks.Last()) < index)
                    {
                        index--;
                    }
                }

                if (tracks.Any(track => collection.IndexOf(track) == index))
                {
                    return;
                }
                
                foreach (var track in tracks)
                {
                    collection.Move(collection.IndexOf(track), index); 
                }
            }
        }
    }
}
