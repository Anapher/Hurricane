using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Hurricane.Music;
using Hurricane.Music.API;
using Hurricane.Settings;
using Hurricane.Settings.RegistryManager;
using Hurricane.Settings.Themes;
using Hurricane.Utilities;
using Hurricane.ViewModelBase;
using Hurricane.Views;

namespace Hurricane.ViewModels
{
    class SettingsViewModel : PropertyChangedBase
    {
        #region "Singleton & Constructor"
        private static SettingsViewModel _instance;
        public static SettingsViewModel Instance
        {
            get { return _instance ?? (_instance = new SettingsViewModel()); }
        }

        private SettingsViewModel()
        {
            RegistryManager = new RegistryManager();
        }

        public void Load()
        {
            Config = ObjectCopier.Clone(HurricaneSettings.Instance.Config);
            AudioDevices = CSCoreEngine.GetAudioDevices();
            SelectedAudioDevice = AudioDevices.FirstOrDefault(x => x.ID == Config.SoundOutDeviceID) ?? AudioDevices.First(x => x.IsDefault);
            CurrentLanguage = Config.Languages.First((x) => x.Code == Config.Language);

            OnPropertyChanged("CanApply");
        }

        public MusicManager MusicManager { get { return MainViewModel.Instance.MusicManager; } }
        public TcpServer ApiServer { get { return MusicManager != null ? MusicManager.ApiServer : null; } }

        public void StateChanged()
        {
            OnPropertyChanged("CanApply");
        }
        #endregion

        private RelayCommand _applychanges;
        public RelayCommand ApplyChanges
        {
            get
            {
                return _applychanges ?? (_applychanges = new RelayCommand(parameter =>
                {
                    ConfigSettings original = HurricaneSettings.Instance.Config;
                    HurricaneSettings.Instance.Config = Config;
                    if (Config.SoundOutDeviceID != original.SoundOutDeviceID) { MusicManager.CSCoreEngine.UpdateSoundOut(); }
                    if (original.Language != Config.Language) { Config.LoadLanguage(); }
                    if (Config.Theme.UseCustomSpectrumAnalyzerColor && string.IsNullOrEmpty(Config.Theme.SpectrumAnalyzerHexColor)) Config.Theme.SpectrumAnalyzerColor = Colors.Black;
                    if (original.Theme != Config.Theme) { Config.Theme.LoadTheme(); }
                    if (original.ApiIsEnabled != Config.ApiIsEnabled) { if (Config.ApiIsEnabled) { ApiServer.StartListening(); } else { ApiServer.StopListening(); } }
                    Config = ObjectCopier.Clone<ConfigSettings>(HurricaneSettings.Instance.Config);
                    OnPropertyChanged("CanApply");
                    CurrentLanguage = Config.Languages.First((x) => x.Code == Config.Language);
                    OnPropertyChanged("ApiState");
                }));
            }
        }

        public bool CanApply
        {
            get
            {
                return !HurricaneSettings.Instance.Config.Equals(Config); //It can apply if something isnt equal
            }
        }

        private ConfigSettings _config;
        public ConfigSettings Config
        {
            get { return _config; }
            set
            {
                SetProperty(value, ref _config);
            }
        }

        private int _selectedtab;
        public int SelectedTab
        {
            get { return _selectedtab; }
            set
            {
                SetProperty(value, ref _selectedtab);
                if (value == 3) OnPropertyChanged("ApiState");
            }
        }

        private List<AudioDevice> _audiodevices;
        public List<AudioDevice> AudioDevices
        {
            get { return _audiodevices; }
            set
            {
                SetProperty(value, ref _audiodevices);
                OnPropertyChanged("CanApply");
            }
        }


        private AudioDevice _selectedaudiodevice;
        public AudioDevice SelectedAudioDevice
        {
            get { return _selectedaudiodevice; }
            set
            {
                if (SetProperty(value, ref _selectedaudiodevice) && value != null)
                {
                    Config.SoundOutDeviceID = value.ID;
                    OnPropertyChanged("CanApply");
                }
            }
        }

        private LanguageInfo _currentlanguage;
        public LanguageInfo CurrentLanguage
        {
            get { return _currentlanguage; }
            set
            {
                if (SetProperty(value, ref _currentlanguage) && value != null)
                {
                    Config.Language = value.Code;
                    OnPropertyChanged("CanApply");
                }
            }
        }

        private RelayCommand _showlanguages;
        public RelayCommand ShowLanguages
        {
            get
            {
                return _showlanguages ?? (_showlanguages = new RelayCommand(parameter => { SelectedTab = 5; }));
            }
        }

        private RelayCommand _testnotification;
        public RelayCommand TestNotification
        {
            get
            {
                return _testnotification ?? (_testnotification = new RelayCommand(parameter => { MusicManager.Notification.Test(Config.Notification); }));
            }
        }

        private RelayCommand _resettrackimport;
        public RelayCommand ResetTrackImport
        {
            get
            {
                return _resettrackimport ?? (_resettrackimport = new RelayCommand(parameter =>
                {
                    Config.RememberTrackImportPlaylist = false;
                    Config.PlaylistToImportTrack = null;
                    OnPropertyChanged("Config");
                    StateChanged();
                }));
            }
        }

        private RelayCommand _totalreset;
        public RelayCommand TotalReset
        {
            get
            {
                return _totalreset ?? (_totalreset = new RelayCommand(parameter =>
                {
                    Config.SetStandardValues();
                    SelectedAudioDevice = AudioDevices[0];
                    OnPropertyChanged("Config");
                    StateChanged();
                }));
            }
        }

        private RelayCommand _openthemecreator;
        public RelayCommand OpenThemeCreator
        {
            get
            {
                return _openthemecreator ?? (_openthemecreator = new RelayCommand(parameter =>
                {
                    ThemeCreatorWindow window = new ThemeCreatorWindow() { Owner = Application.Current.MainWindow };
                    if (window.ShowDialog() == true)
                        Config.Theme.RefreshThemes();
                }));
            }
        }

        private RelayCommand _edittheme;
        public RelayCommand EditTheme
        {
            get
            {
                return _edittheme ?? (_edittheme = new RelayCommand(parameter =>
                {
                    var source = new ThemeSource();
                    var theme = Config.Theme.SelectedColorTheme as CustomColorTheme;
                    if (theme == null) return;
                    source.LoadFromFile(theme.Filename);
                    source.Name = Path.GetFileNameWithoutExtension(theme.Filename);
                    ThemeCreatorWindow window = new ThemeCreatorWindow(source) { Owner = Application.Current.MainWindow };
                    window.ShowDialog();
                    if (theme.Name == HurricaneSettings.Instance.Config.Theme.SelectedColorTheme.Name)
                    {
                        theme.RefreshResource();
                        theme.ApplyTheme();
                    }
                }));
            }
        }

        public string ApiState
        {
            get
            {
                if (ApiServer == null) return Application.Current.FindResource("Deactivated").ToString();
                if (ApiServer.IsRunning) { return Application.Current.FindResource("Activated").ToString(); }
                return ApiServer.GotProblems ? Application.Current.FindResource("FailedToBindToPort").ToString() : Application.Current.FindResource("Deactivated").ToString();
            }
        }

        public RegistryManager RegistryManager { get; set; }
    }
}