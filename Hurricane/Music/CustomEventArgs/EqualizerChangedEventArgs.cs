using System;

namespace Hurricane.Music.CustomEventArgs
{
   public class EqualizerChangedEventArgs : EventArgs
    {
        public int EqualizerNumber { get; protected set; }
        public double EqualizerValue { get; set; }

        public EqualizerChangedEventArgs(int equalizernumber, double equalizervalue)
        {
            EqualizerNumber = equalizernumber;
            EqualizerValue = equalizervalue;
        }
    }
}
