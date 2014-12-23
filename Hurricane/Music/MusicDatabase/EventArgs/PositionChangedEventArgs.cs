using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music
{
    public class PositionChangedEventArgs : EventArgs
    {
        public int NewPosition { get; protected set; }
        public int TrackLength { get; protected set; }

        public PositionChangedEventArgs(int newposition, int tracklength)
        {
            this.NewPosition = newposition;
            this.TrackLength = tracklength;
        }
    }
}
