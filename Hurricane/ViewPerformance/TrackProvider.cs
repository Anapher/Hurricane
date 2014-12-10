using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurricane.Music;
using System.Collections.ObjectModel;

namespace Hurricane.ViewPerformance
{
    class TrackProvider : IItemsProvider<Track>
    {
        private readonly int _count;
        private readonly ObservableCollection<Track> BaseList;

        public int FetchCount()
        {
            return _count;
        }

        public TrackProvider(int count, ObservableCollection<Track> baselist)
        {
            this._count = count;
            this.BaseList = baselist;
        }

        public IList<Track> FetchRange(int startIndex, int count)
        {
            System.Diagnostics.Debug.Print("FetchRange: " + startIndex + "," + count);

            List<Track> list = new List<Track>();
            for (int i = startIndex; i < startIndex + count; i++)
            {
                if (i > _count -1) break;
                list.Add(BaseList[i]);
            }

            return list;
        }
    }
}
