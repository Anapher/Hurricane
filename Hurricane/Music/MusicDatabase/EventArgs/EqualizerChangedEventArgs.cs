namespace Hurricane.Music.MusicDatabase.EventArgs
{
   public class EqualizerChangedEventArgs : System.EventArgs
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
