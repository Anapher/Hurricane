using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using Hurricane.Music.Playlist;

namespace Hurricane.Settings
{
    [Serializable, XmlType(TypeName = "Playlists")]
    public class PlaylistSettings : SettingsBase
    {
        protected const string Filename = "playlists.xml";

        [XmlElement("List")]
        public ObservableCollection<NormalPlaylist> Playlists { get; set; }

        public override void SetStandardValues()
        {
            Playlists = new ObservableCollection<NormalPlaylist> { new NormalPlaylist() { Name = "Default" } };
        }

        public override void Save(string programPath)
        {
            Save<PlaylistSettings>(Path.Combine(programPath, Filename));
        }

        public static PlaylistSettings Load(string programpath)
        {
            var fi = new FileInfo(Path.Combine(programpath, Filename));
            if (!fi.Exists || string.IsNullOrWhiteSpace(File.ReadAllText(fi.FullName)))
            {
                var result = new PlaylistSettings();
                result.SetStandardValues();
                return result;
            }

            using (StreamReader reader = new StreamReader(fi.FullName))
            {
                var deserializer = new XmlSerializer(typeof(PlaylistSettings));
                return (PlaylistSettings)deserializer.Deserialize(reader);
            }
        }
    }
}