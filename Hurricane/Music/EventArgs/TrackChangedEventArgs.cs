using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music
{
  public  class TrackChangedEventArgs : EventArgs
    {
        private Track newtrack;
        public Track NewTrack
        {
            get { return newtrack; }
            protected set { newtrack = value; }
        }
        
        public TrackChangedEventArgs(Track newtrack)
        {
            this.NewTrack = newtrack;
        }
    }
}
