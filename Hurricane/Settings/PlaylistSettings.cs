using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Collections.ObjectModel;

namespace Hurricane.Settings
{
    [XmlType(TypeName = "Playlists")]
    public class PlaylistSettings : SettingsBase
    {
        protected const string Filename = "Playlists.xml";

        public ObservableCollection<Music.Playlist> Playlists { get; set; }

        public override void SetStandardValues()
        {
            Playlists = new ObservableCollection<Music.Playlist>();
            Playlists.Add(new Music.Playlist() { Name = "Default" });
        }

        public override void Save(string ProgramPath)
        {
            this.Save<PlaylistSettings>(Path.Combine(ProgramPath, Filename));
        }

        public static PlaylistSettings Load(string Programpath)
        {
            FileInfo fi = new FileInfo(Path.Combine(Programpath, Filename));
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
