using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Model.Music.Playlist
{
    public class History
    {
        public History()
        {
            HistoryEntries = new ObservableCollection<HistoryEntry>();
        }

        public ObservableCollection<HistoryEntry> HistoryEntries { get; set; }

        public void AddEntry(IPlayable playable, TimeSpan timePlayed)
        {
            var entry = new HistoryEntry
            {
                Timestamp = DateTime.Now,
                ArtistName = playable.Artist,
                TrackTitle = playable.Title,
                TimePlayed = timePlayed
            };

            var playableBase = playable as PlayableBase;
            if (playableBase != null)
            {
                entry.ArtistId = playableBase.ArtistGuid;
                //entry.TrackId = _trackProvider.Collection.First(x => x.Value == playableBase).Key;
            }

            HistoryEntries.Insert(0, entry);
        }
    }

    public class HistoryEntry
    {
        public string Genre { get; set; }
        public string TrackTitle { get; set; }
        public string ArtistName { get; set; }
        public DateTime Timestamp { get; set; }

        public Guid TrackId { get; set; }
        public Guid ArtistId { get; set; }

        [XmlIgnore]
        public TimeSpan TimePlayed { get; set; }

        // XmlSerializer does not support TimeSpan, so use this property for serialization instead.
        [Browsable(false)]
        [XmlElement(DataType = "duration", ElementName = "TimePlayed")]
        public string TimePlayedString
        {
            get
            {
                return XmlConvert.ToString(TimePlayed);
            }
            set
            {
                TimePlayed = string.IsNullOrEmpty(value)
                    ? TimeSpan.Zero
                    : XmlConvert.ToTimeSpan(value);
            }
        }
    }
}