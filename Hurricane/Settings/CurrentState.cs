using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurricane.MagicArrow.DockManager;
using Hurricane.Music;
using Hurricane.Music.Download;
using Hurricane.Music.MusicEqualizer;
using Hurricane.ViewModelBase;
using System.IO;
using System.Xml.Serialization;

namespace Hurricane.Settings
{
    [Serializable]
    public class CurrentState : SettingsBase
    {
        private const string Filename = "current.xml";

        public float Volume { get; set; }

        public long TrackPosition { get; set; }
        public int LastPlaylistIndex { get; set; }
        public int LastTrackIndex { get; set; }
        public int SelectedPlaylist { get; set; }
        public int SelectedTrack { get; set; }
        public QueueManager Queue { get; set; }
        public bool IsLoopEnabled { get; set; }
        public bool IsShuffleEnabled { get; set; }
        public EqualizerSettings EqualizerSettings { get; set; }
        public DockingApplicationState ApplicationState { get; set; }

        private bool _equalizerIsOpen;
        public bool EqualizerIsOpen
        {
            get { return _equalizerIsOpen; }
            set
            {
                SetProperty(value, ref _equalizerIsOpen);
            }
        }

        public CurrentState()
        {
            SetStandardValues();
        }

        public override sealed void SetStandardValues()
        {
            LastPlaylistIndex = -10;
            LastTrackIndex = -1;
            TrackPosition = 0;
            Volume = 1.0f;
            SelectedPlaylist = 0;
            SelectedTrack = -1;
            IsLoopEnabled = false;
            IsShuffleEnabled = false;
            ApplicationState = null;
            EqualizerSettings = new EqualizerSettings();
            EqualizerSettings.CreateNew();
        }

        public static CurrentState Load(string programpath)
        {
            var fi = new FileInfo(Path.Combine(programpath, Filename));
            if (!fi.Exists || string.IsNullOrWhiteSpace(File.ReadAllText(fi.FullName))) return new CurrentState();

            using (var reader = new StreamReader(Path.Combine(programpath, Filename)))
            {
                var deserializer = new XmlSerializer(typeof(CurrentState));
                return (CurrentState)deserializer.Deserialize(reader);
            }
        }

        public override void Save(string programPath)
        {
            Save<CurrentState>(Path.Combine(programPath, Filename));
        }
    }
}