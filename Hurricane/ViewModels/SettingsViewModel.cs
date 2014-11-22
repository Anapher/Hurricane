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
        }

        private MahApps.Metro.Controls.MetroWindow BaseWindow;
        public void Load(MahApps.Metro.Controls.MetroWindow window)
        {
            this.BaseWindow = window;
            Config = Utilities.ObjectCopier.Clone<Settings.ConfigSettings>(Settings.HurricaneSettings.Instance.Config);
            AudioDevices = Music.CSCoreEngine.GetAudioDevices();
            SelectedAudioDevice = AudioDevices.Where((x) => x.ID == Config.SoundOutDeviceID).FirstOrDefault();
            if (SelectedAudioDevice == null) SelectedAudioDevice = AudioDevices.Where((x) => x.IsDefault).First();
            CurrentLanguage = Config.Languages.First((x) => x.Code == Config.Language);

            OnPropertyChanged("CanApply");
        }

        public Music.MusicManager MusicManager { get; set; }

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
                        if (original.Theme != Config.Theme) { Config.LoadTheme(); }
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

        private RelayCommand close;
        public RelayCommand Close
        {
            get
            {
                if (close == null)
                    close = new RelayCommand((object parameter) => { BaseWindow.Close(); });
                return close;
            }
        }
        
        private List<Music.CSCoreEngine.AudioDevice> audiodevices;
        public List<Music.CSCoreEngine.AudioDevice> AudioDevices
        {
            get { return audiodevices; }
            set
            {
                SetProperty(value, ref audiodevices);
                OnPropertyChanged("CanApply");
            }
        }

        
        private Music.CSCoreEngine.AudioDevice selectedaudiodevice;
        public Music.CSCoreEngine.AudioDevice SelectedAudioDevice
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
                    showlanguages = new RelayCommand((object parameter) => { SelectedTab = 2; });
                return showlanguages;
            }
        }
    }
}
