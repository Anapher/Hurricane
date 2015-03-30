using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Hurricane.Designer.Data;
using Hurricane.Settings.Themes.AudioVisualisation;
using Hurricane.Settings.Themes.AudioVisualisation.DefaultAudioVisualisation;
using Hurricane.Settings.Themes.Visual.AccentColors;
using Hurricane.Settings.Themes.Visual.AppThemes;
using MahApps.Metro;
using Application = System.Windows.Application;
using AppTheme = Hurricane.Settings.Themes.Visual.AppThemes.AppTheme;

namespace Hurricane.Settings.Themes
{
    public class ApplicationThemeManager
    {
        #region "Singleton & Constructor"

        private static ApplicationThemeManager _instance;
        public static ApplicationThemeManager Instance
        {
            get { return _instance ?? (_instance = new ApplicationThemeManager()); }
        }


        private ApplicationThemeManager()
        {
            _loadedResources = new Dictionary<string, ResourceDictionary>();
        }

        #endregion

        private ObservableCollection<AccentColorBase> _accentColors;
        public ObservableCollection<AccentColorBase> AccentColors
        {
            get
            {
                return _accentColors;
            }
        }

        private ObservableCollection<AppThemeBase> _appThemes;
        public ObservableCollection<AppThemeBase> AppThemes
        {
            get
            {
                return _appThemes;
            }
        }

        private ObservableCollection<ThemePack> _themePacks;
        public ObservableCollection<ThemePack> ThemePacks
        {
            get
            {
                return _themePacks;
            }
        }

        private ObservableCollection<IAudioVisualisationContainer> _audioVisualisations;
        public ObservableCollection<IAudioVisualisationContainer> AudioVisualisations
        {
            get
            {
                return _audioVisualisations;
            }
        }

        public void Refresh()
        {
            _accentColors = new ObservableCollection<AccentColorBase>();
            _appThemes = new ObservableCollection<AppThemeBase>();
            _themePacks = new ObservableCollection<ThemePack>();
            _audioVisualisations = new ObservableCollection<IAudioVisualisationContainer>();

            foreach (var t in ThemeManager.Accents.Select(a => new AccentColor { Name = a.Name }).OrderBy(x => x.TranslatedName))
            {
                _accentColors.Add(t);
            }

            foreach (var t in ThemeManager.AppThemes.Select(a => new AppTheme { Name = a.Name }).OrderBy(x => x.TranslatedName))
            {
                _appThemes.Add(t);
            }

            _audioVisualisations.Add(DefaultAudioVisualisation.GetDefault());

            var accentColorsFolder = new DirectoryInfo(HurricaneSettings.Paths.AccentColorsDirectory);
            if (accentColorsFolder.Exists)
            {
                foreach (var file in accentColorsFolder.GetFiles("*.xaml"))
                {
                    CustomAccentColor theme;

                    if (CustomAccentColor.FromFile(file.FullName, out theme))
                        _accentColors.Add(theme);
                }
            }

            var appThemesFolder = new DirectoryInfo(HurricaneSettings.Paths.AppThemesDirectory);
            if (appThemesFolder.Exists)
            {
                foreach (var file in appThemesFolder.GetFiles("*.xaml"))
                {
                    CustomAppTheme theme;
                    if (CustomAppTheme.FromFile(file.FullName, out theme))
                        _appThemes.Add(theme);
                }
            }

            var themePacksFolder = new DirectoryInfo(HurricaneSettings.Paths.ThemePacksDirectory);
            if (themePacksFolder.Exists)
            {
                foreach (var file in themePacksFolder.GetFiles("*.htp"))
                {
                    ThemePack pack;
                    if (ThemePack.FromFile(file.FullName, out pack))
                    {
                        _themePacks.Add(pack);
                    }
                }
            }

            var audioVisualisations = new DirectoryInfo(HurricaneSettings.Paths.AudioVisualisationsDirectory);
            if (audioVisualisations.Exists)
            {
                foreach (var file in audioVisualisations.GetFiles("*.dll"))
                {
                    _audioVisualisations.Add(new CustomAudioVisualisation { FileName = file.Name });
                }
            }
        }

        public ThemePack GetThemePack(string name)
        {
            return null;
        }

        public void Apply(ApplicationDesign design)
        {
            try
            {
                design.AccentColor.ApplyTheme();
            }
            catch (Exception)
            {
                design.AccentColor = AccentColors.First(x => x.Name == "Blue");
                design.AccentColor.ApplyTheme();
            }

            try
            {
                design.AppTheme.ApplyTheme();
            }
            catch (Exception)
            {
                design.AppTheme = AppThemes.First();
                design.AppTheme.ApplyTheme();
            }

            if (design.AudioVisualisation != null)
                design.AudioVisualisation.AudioVisualisationPlugin.Refresh();
        }

        private readonly Dictionary<string, ResourceDictionary> _loadedResources;
        public void LoadResource(string key, ResourceDictionary resource)
        {
            Application.Current.Resources.MergedDictionaries.Add(resource);

            if (_loadedResources.ContainsKey(key))
            {
                Application.Current.Resources.MergedDictionaries.Remove(_loadedResources[key]);
                _loadedResources.Remove(key);
            }

            _loadedResources.Add(key, resource);
        }
    }
}