using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using Hurricane.Music;

namespace Hurricane.Settings
{
    [XmlType(TypeName = "Playlists")]
    public class PlaylistSettings : SettingsBase
    {
        protected const string Filename = "Playlists.xml";

        public ObservableCollection<Playlist> Playlists { get; set; }

        public override void SetStandardValues()
        {
            Playlists = new ObservableCollection<Playlist> { new Playlist() { Name = "Default" } };
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
