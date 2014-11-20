using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace Hurricane.Settings
{
    public class HurricaneSettings
    {
        private static HurricaneSettings _instance;
        public static HurricaneSettings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new HurricaneSettings();
                return _instance;
            }
        }

        private string ProgramPath;
        private HurricaneSettings()
        {
            ProgramPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        public PlaylistSettings Playlists { get; set; }
        public ConfigSettings Config { get; set; }

        public bool Loaded { get; set; }

        public void Load()
        {
            Playlists = PlaylistSettings.Load(ProgramPath);
            Config = ConfigSettings.Load(ProgramPath);
            this.Loaded = true;
        }

        public void Save()
        {
            if (Playlists != null) Playlists.Save(ProgramPath);
            if (Config != null) Config.Save(ProgramPath);
        }
    }
}
