using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Hurricane.Model;
using Hurricane.Model.AudioEngine;
using Hurricane.Model.Music;
using Hurricane.Model.Settings;
using Hurricane.ViewModel.Settings;
using MahApps.Metro;

namespace Hurricane.ViewModel
{
    public class SettingsViewModel : PropertyChangedBase
    {
        private AccentColorData _selectedAccentColor;
        private ThemeData _selectedTheme;

        private RelayCommand _applySoundOutDeviceCommand;
        private readonly Action _refreshViewAction;
        private readonly SettingsData _settings = SettingsManager.Current;

        public SettingsViewModel(MusicDataManager musicDataManager, Action refreshViewAction)
        {
            MusicDataManager = musicDataManager;
            _refreshViewAction = refreshViewAction;

            SelectedSoundOutDevice = musicDataManager.MusicManager.AudioEngine.SoundOutProvider.CurrentSoundOutDevice;
            AccentColors =
                ThemeManager.Accents.Select(
                    x => new AccentColorData {ColorBrush = x.Resources["AccentColorBrush"] as Brush, Key = x.Name, AccentColor = x})
                    .ToList();
            Themes = ThemeManager.AppThemes.Select(x => new ThemeData {Key = x.Name, Theme = x}).ToList();

            SelectedTheme = Themes.FirstOrDefault(x => x.Key == _settings.Theme) ?? Themes.First();
            SelectedAccentColor = AccentColors.FirstOrDefault(x => x.Key == _settings.AccentColor) ??
                                   AccentColors.First();
        }

        public MusicDataManager MusicDataManager { get; }
        public ISoundOutDevice SelectedSoundOutDevice { get; set; }
        public List<AccentColorData> AccentColors { get; }
        public List<ThemeData> Themes { get; }

        public AccentColorData SelectedAccentColor
        {
            get { return _selectedAccentColor; }
            set
            {
                if (_selectedAccentColor != value)
                {
                    _selectedAccentColor = value;
                    var theme = ThemeManager.DetectAppStyle(Application.Current);
                    ThemeManager.ChangeAppStyle(Application.Current, value.AccentColor, theme.Item1);
                    _refreshViewAction.Invoke();
                    _settings.AccentColor = value.Key;
                }
            }
        }

        public ThemeData SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                if (_selectedTheme != value)
                {
                    _selectedTheme = value;
                    var theme = ThemeManager.DetectAppStyle(Application.Current);
                    ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, value.Theme);
                    _refreshViewAction.Invoke();
                    _settings.Theme = value.Key;
                }
            }
        }

        public RelayCommand ApplySoundOutDeviceCommand
        {
            get
            {
                return _applySoundOutDeviceCommand ?? (_applySoundOutDeviceCommand = new RelayCommand(parameter =>
                {
                    MusicDataManager.MusicManager.AudioEngine.SoundOutProvider.CurrentSoundOutDevice =
                        SelectedSoundOutDevice;
                }));
            }
        }
    }
}