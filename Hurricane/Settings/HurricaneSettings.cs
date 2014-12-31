using System;

namespace Hurricane.Settings
{
    public class HurricaneSettings
    {
        private static HurricaneSettings _instance;
        public static HurricaneSettings Instance
        {
            get { return _instance ?? (_instance = new HurricaneSettings()); }
        }

        private readonly string _programPath;
        private HurricaneSettings()
        {
            _programPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        public PlaylistSettings Playlists { get; set; }
        public ConfigSettings Config { get; set; }

        public bool Loaded { get; set; }

        public void Load()
        {
            Playlists = PlaylistSettings.Load(_programPath);
            Config = ConfigSettings.Load(_programPath);
            this.Loaded = true;
        }

        public void Save()
        {
            if (Playlists != null) Playlists.Save(_programPath);
            if (Config != null) Config.Save(_programPath);
        }
    }
}
