using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurricane.ViewModelBase;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Hurricane.Settings;

namespace Hurricane.ViewModels
{
    class SettingsViewModel : PropertyChangedBase
    {
        #region "Singleton & Constructor"
        private static SettingsViewModel _instance;
        public static SettingsViewModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SettingsViewModel();
                return _instance;
            }
        }

        private SettingsViewModel()
        {
            RegistryManager = new Settings.RegistryManager.RegistryManager();
        }

        public void Load()
        {
            Config = Utilities.ObjectCopier.Clone<Settings.ConfigSettings>(Settings.HurricaneSettings.Instance.Config);
            AudioDevices = Music.CSCoreEngine.GetAudioDevices();
            SelectedAudioDevice = AudioDevices.Where((x) => x.ID == Config.SoundOutDeviceID).FirstOrDefault();
            if (SelectedAudioDevice == null) SelectedAudioDevice = AudioDevices.Where((x) => x.IsDefault).First();
            CurrentLanguage = Config.Languages.First((x) => x.Code == Config.Language);

            OnPropertyChanged("CanApply");
        }

        public Music.MusicManager MusicManager { get { return ViewModels.MainViewModel.Instance.MusicManager; } }

        public void StateChanged()
        {
            OnPropertyChanged("CanApply");
        }
        #endregion

        private RelayCommand applychanges;
        public RelayCommand ApplyChanges
        {
            get
            {
                if (applychanges == null)
                    applychanges = new RelayCommand((object parameter) =>
                    {
                        Settings.ConfigSettings original = Settings.HurricaneSettings.Instance.Config;
                        Settings.HurricaneSettings.Instance.Config = Config;
                        if (Config.SoundOutDeviceID != original.SoundOutDeviceID) { MusicManager.CSCoreEngine.UpdateSoundOut(); }
                        if (original.Language != Config.Language) { Config.LoadLanguage(); }
                        if (Config.Theme.UseCustomSpectrumAnalyzerColor && string.IsNullOrEmpty(Config.Theme.SpectrumAnalyzerHexColor)) Config.Theme.SpectrumAnalyzerColor = System.Windows.Media.Colors.Black;
                        if (original.Theme != Config.Theme) { Config.Theme.LoadTheme(); }
                        Config = Utilities.ObjectCopier.Clone<Settings.ConfigSettings>(Settings.HurricaneSettings.Instance.Config);
                        OnPropertyChanged("CanApply");
                        CurrentLanguage = Config.Languages.First((x) => x.Code == Config.Language);
                    });
                return applychanges;
            }
        }

        public bool CanApply
        {
            get
            {
                return !Settings.HurricaneSettings.Instance.Config.Equals(Config); //It can apply if something isnt equal
            }
        }
        
        private Settings.ConfigSettings config;
        public Settings.ConfigSettings Config
        {
            get { return config; }
            set
            {
                SetProperty(value, ref config);
            }
        }
        
        private int selectedtab = 0;
        public int SelectedTab
        {
            get { return selectedtab; }
            set
            {
                SetProperty(value, ref selectedtab);
            }
        }

        private List<Music.AudioDevice> audiodevices;
        public List<Music.AudioDevice> AudioDevices
        {
            get { return audiodevices; }
            set
            {
                SetProperty(value, ref audiodevices);
                OnPropertyChanged("CanApply");
            }
        }

        
        private Music.AudioDevice selectedaudiodevice;
        public Music.AudioDevice SelectedAudioDevice
        {
            get { return selectedaudiodevice; }
            set
            {
                if (SetProperty(value, ref selectedaudiodevice) && value != null)
                {
                    Config.SoundOutDeviceID = value.ID;
                    OnPropertyChanged("CanApply");
                }
            }
        }

        private LanguageInfo currentlanguage;
        public LanguageInfo CurrentLanguage
        {
            get { return currentlanguage; }
            set
            {
                if (SetProperty(value, ref currentlanguage) && value != null)
                {
                    Config.Language = value.Code;
                    OnPropertyChanged("CanApply");
                }
            }
        }

        private RelayCommand showlanguages;
        public RelayCommand ShowLanguages
        {
            get
            {
                if (showlanguages == null)
                    showlanguages = new RelayCommand((object parameter) => { SelectedTab = 5; });
                return showlanguages;
            }
        }

        private RelayCommand testnotification;
        public RelayCommand TestNotification
        {
            get
            {
                if (testnotification == null)
                    testnotification = new RelayCommand((object parameter) => { MusicManager.Notification.Test(Config.Notification); });
                return testnotification;
            }
        }

        private RelayCommand resettrackimport;
        public RelayCommand ResetTrackImport
        {
            get
            {
                if (resettrackimport == null)
                    resettrackimport = new RelayCommand((object parameter) => { Config.RememberTrackImportPlaylist = false; Config.PlaylistToImportTrack = null; OnPropertyChanged("Config"); StateChanged(); });
                return resettrackimport;
            }
        }

        private RelayCommand totalreset;
        public RelayCommand TotalReset
        {
            get
            {
                if (totalreset == null)
                    totalreset = new RelayCommand((object parameter) => { Config.SetStandardValues(); SelectedAudioDevice = AudioDevices[0]; OnPropertyChanged("Config"); StateChanged(); });
                return totalreset;
            }
        }

        public Settings.RegistryManager.RegistryManager RegistryManager { get; set; }
    }
}
