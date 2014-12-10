using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using DevZest.Windows.DataVirtualization;
using Hurricane.Music;
using System.Collections.ObjectModel;
/*
namespace Hurricane.ViewPerformance
{
    class TrackLoader : IVirtualListLoader<Track>
    {

        public bool CanSort
        {
            get { return false; }
        }

        protected ObservableCollection<Track> _tracks;
        public TrackLoader(ObservableCollection<Track> tracks)
        {
            _tracks = tracks;
        }

        public IList<Track> LoadRange(int startIndex, int count, System.ComponentModel.SortDescriptionCollection sortDescriptions, out int overallCount)
        {
            Track[] tracks = new Track[count];
            for (int i = 0; i < count; i++)
            {
                int index;
                index = startIndex + i;

                if (index > _tracks.Count - 1) break;
                tracks[i] = _tracks[index];
            }

            overallCount = _tracks.Count;

            return tracks;
        }
    }
}
*/