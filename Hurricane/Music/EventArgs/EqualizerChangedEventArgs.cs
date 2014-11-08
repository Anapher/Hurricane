using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music
{
   public class EqualizerChangedEventArgs : EventArgs
    {
        public int EqualizerNumber { get; protected set; }
        public double EqualizerValue { get; set; }

        public EqualizerChangedEventArgs(int equalizernumber, double equalizervalue)
        {
            this.EqualizerNumber = equalizernumber;
            this.EqualizerValue = equalizervalue;
        }
    }
}
