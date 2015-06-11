using System;
using Hurricane.Utilities;

namespace Hurricane.Model.Settings
{
    public class SettingsData
    {
        public string SoundOutDevice { get; set; }
        public string SoundOutMode { get; set; }
        public float Volume { get; set; }
        public long TrackPosition { get; set; }
        public Guid CurrentTrack { get; set; }
        public Guid CurrentPlaylist { get; set; }

        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public double WindowLeft { get; set; }
        public double WindowTop { get; set; }

        public string AccentColor { get; set; }
        public string Theme { get; set; }

        public void Initalize()
        {
            Theme = "BaseLight";
            AccentColor = "Cyan";
            WindowWidth = 900;
            WindowHeight = 600;
            WindowLeft = (WpfScreen.Primary.WorkingArea.Width - WindowWidth) / 2;
            WindowHeight = (WpfScreen.Primary.WorkingArea.Height - WindowHeight) / 2;
            Volume = .7f;
        }
    }
}