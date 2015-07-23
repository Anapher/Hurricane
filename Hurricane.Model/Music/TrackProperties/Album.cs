using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Hurricane.Model.Music.TrackProperties
{
    /// <summary>
    /// Provides information about an album
    /// </summary>
    [Serializable]
    public class Album
    {
        public Album()
        {
            Artists = new ObservableCollection<Artist>();
        }

        /// <summary>
        /// The name of the album
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// The id of the album
        /// </summary>
        [XmlAttribute]
        public Guid Guid { get; set; }

        /// <summary>
        /// The artists of the album
        /// </summary>
        public ObservableCollection<Artist> Artists { get; set; }
    }
}