using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Hurricane.Model;
using Hurricane.Model.AudioEngine;
using Hurricane.Model.Music;
using Hurricane.ViewModel.Settings;
using MahApps.Metro;

namespace Hurricane.ViewModel
{
    public class SettingsViewModel : PropertyChangedBase
    {
        private AccentColorData _selectedAccentColor;

        private RelayCommand _applySoundOutDeviceCommand;

        public SettingsViewModel(MusicDataManager musicDataManager)
        {
            MusicDataManager = musicDataManager;
            SelectedSoundOutDevice = musicDataManager.MusicManager.AudioEngine.SoundOutProvider.CurrentSoundOutDevice;
            AccentColors =
                ThemeManager.Accents.Select(
                    x => new AccentColorData {ColorBrush = x.Resources["AccentColorBrush"] as Brush, Key = x.Name, AccentColor = x})
                    .ToList();
            Themes = ThemeManager.AppThemes.Select(x => new ThemeData {Key = x.Name}).ToList();
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