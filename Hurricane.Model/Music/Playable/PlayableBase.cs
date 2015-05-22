using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using Hurricane.Model.AudioEngine;

namespace Hurricane.Model.Music.Playable
{
    public abstract class PlayableBase : PropertyChangedBase, IPlayable
    {
        private string _title;
        private string _artist;
        private string _album;

        public string Title
        {
            get { return _title; }
            set { SetProperty(value, ref _title); }
        }

        public string Artist
        {
            get { return _artist; }
            set { SetProperty(value, ref _artist); }
        }

        public string Album
        {
            get { return _album; }
            set { SetProperty(value, ref _album); }
        }

        [XmlIgnore]
        public TimeSpan Duration { get; set; }

        // XmlSerializer does not support TimeSpan, so use this property for serialization instead.
        [Browsable(false)]
        [XmlElement(DataType = "duration", ElementName = "Duration")]
        public string DurationString
        {
            get
            {
                return XmlConvert.ToString(Duration);   
            }
            set
            {
                Duration = string.IsNullOrEmpty(value)
                    ? TimeSpan.Zero
                    : XmlConvert.ToTimeSpan(value);
            }
        }

        public BitmapImage Cover { get; protected set; }
        public abstract bool IsAvailable { get; }

        public abstract Task<IPlaySource> GetSoundSource();
        public abstract Task LoadImage();
    }
}