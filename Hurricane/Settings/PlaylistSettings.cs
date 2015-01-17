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
            this.Save<PlaylistSettings>(Path.Combine(programPath, Filename));
        }

        public static PlaylistSettings Load(string programpath)
        {
            FileInfo fi = new FileInfo(Path.Combine(programpath, Filename));
            if (fi.Exists)
            {
                using (StreamReader reader = new StreamReader(fi.FullName))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(PlaylistSettings));
                    return (PlaylistSettings)deserializer.Deserialize(reader);
                }
            }
            else
            {
                PlaylistSettings result = new PlaylistSettings();
                result.SetStandardValues();
                return result;
            }

        }
    }
}
