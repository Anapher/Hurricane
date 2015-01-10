using System;
using System.IO;

namespace Hurricane.Settings
{
    public class HurricaneSettings
    {
        public readonly string BaseDirectory;
        public readonly string ThemeDirectory;
        public readonly string CoverDirectory;

        private static HurricaneSettings _instance;
        public static HurricaneSettings Instance
        {
            get { return _instance ?? (_instance = new HurricaneSettings()); }
        }

        public PlaylistSettings Playlists { get; set; }
        public ConfigSettings Config { get; set; }

        public bool IsLoaded { get; set; }

        public HurricaneSettings()
        {
            if (File.Exists(".IsInstalled"))
            {
                var appDataDir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hurricane"));
                if (!appDataDir.Exists) appDataDir.Create();
                BaseDirectory = appDataDir.FullName;
            }
            else
            {
                BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }
            ThemeDirectory = Path.Combine(BaseDirectory, "Themes");
            CoverDirectory = Path.Combine(BaseDirectory, "AlbumCover");
        }

        public void Load()
        {
            Playlists = PlaylistSettings.Load(BaseDirectory);
            Config = ConfigSettings.Load(BaseDirectory);
            IsLoaded = true;
        }

        public void Save()
        {
            if (Playlists != null) Playlists.Save(BaseDirectory);
            if (Config != null) Config.Save(BaseDirectory);
        }
    }
}
