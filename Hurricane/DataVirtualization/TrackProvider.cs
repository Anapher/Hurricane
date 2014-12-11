using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurricane.Music;
using System.Collections.ObjectModel;

namespace Hurricane.DataVirtualization
{
    class TrackProvider : IItemsProvider<Track>
    {
        public List<Track> BaseList { get; set; }

        public int FetchCount()
        {
            return BaseList.Count;
        }

        public TrackProvider(List<Track> baselist)
        {
            this.BaseList = baselist;
        }

        public IList<Track> FetchRange(int startIndex, int count)
        {
            Track[] tracks = new Track[count];
            for (int i = 0; i < count; i++)
            {
                int index;
                index = startIndex + i;
                if (index > BaseList.Count - 1) break;
                tracks[i] = BaseList[index];
            }

            return tracks;
        }
    }
}
