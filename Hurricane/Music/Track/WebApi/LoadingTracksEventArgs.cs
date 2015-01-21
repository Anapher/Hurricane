using System;

namespace Hurricane.Music.Track.WebApi
{
    public class LoadingTracksEventArgs : EventArgs
    {
        public double Value { get; set; }
        public double Maximum { get; set; }
        public string CurrentTrackName { get; set; }

        public LoadingTracksEventArgs(double value, double maximum, string currentTrackName)
        {
            this.Value = value;
            this.Maximum = maximum;
            this.CurrentTrackName = currentTrackName;
        }
    }
}
