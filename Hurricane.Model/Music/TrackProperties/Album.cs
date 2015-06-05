using System;
using System.Xml.Serialization;

namespace Hurricane.Model.Music.TrackProperties
{
    /// <summary>
    /// Provides information about an album
    /// </summary>
    [Serializable]
    public class Album
    {
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
    }
}