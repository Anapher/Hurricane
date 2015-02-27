using System;
using System.IO;
using Hurricane.Settings.Themes;

namespace Hurricane.Settings
{
    public class HurricaneSettings
    {
        public readonly string BaseDirectory;
        public readonly string CoverDirectory;

        public readonly string AccentColorsDirectory;
        public readonly string AppThemesDirectory;
        public readonly string ThemePacksDirectory;
        public readonly string AudioVisualisationsDirectory;

        private static HurricaneSettings _instance;
        public static HurricaneSettings Instance
        {
            get { return _instance ?? (_instance = new HurricaneSettings()); }
        }

        public PlaylistSettings Playlists { get; set; }
        public ConfigSettings Config { get; set; }
        public CurrentState CurrentState { get; set; }

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
            CoverDirectory = Path.Combine(BaseDirectory, "AlbumCover");

            var themeDirectory = CheckDirectory(Path.Combine(BaseDirectory, "Themes"));
            AccentColorsDirectory = CheckDirectory(Path.Combine(themeDirectory, "AccentColors"));
            AppThemesDirectory = CheckDirectory(Path.Combine(themeDirectory, "AppThemes"));
            ThemePacksDirectory = CheckDirectory(Path.Combine(themeDirectory, "ThemePacks"));
            AudioVisualisationsDirectory = CheckDirectory(Path.Combine(themeDirectory, "AudioVisualisations"));
        }

        private static string CheckDirectory(string path)
        {
            var folder = new DirectoryInfo(path);
            if (!folder.Exists) folder.Create();
            return folder.FullName;
        }

        public void Load()
        {
            ApplicationThemeManager.Instance.Refresh();
            Playlists = PlaylistSettings.Load(BaseDirectory);
            Config = ConfigSettings.Load(BaseDirectory);
            CurrentState = CurrentState.Load(BaseDirectory);
            IsLoaded = true;
        }

        public void Save()
        {
            if (Playlists != null) Playlists.Save(BaseDirectory);
            if (Config != null) Config.Save(BaseDirectory);
            if (CurrentState != null) CurrentState.Save(BaseDirectory);
        }
    }
}
