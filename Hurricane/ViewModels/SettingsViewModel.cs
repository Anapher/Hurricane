using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using CSCore.SoundOut;
using Hurricane.Music;
using Hurricane.Music.Data;
using Hurricane.Settings;
using Hurricane.Settings.RegistryManager;
using Hurricane.Settings.Themes;
using Hurricane.Settings.Themes.Background;
using Hurricane.Settings.Themes.Visual;
using Hurricane.ViewModelBase;
using Microsoft.Win32;
using WPFFolderBrowser;

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
            RegistryManager = new RegistryManager(); //important for shortcut
        }

        public void Load()
        {
            SoundOutList = CSCoreEngine.GetSoundOutList();
            SelectedSoundOut = SoundOutList.First(x => x.SoundOutMode == Config.SoundOutMode);
            CurrentLanguage = Config.Languages.First(x => x.Code == Config.Language);
        }

        public MusicManager MusicManager { get { return MainViewModel.Instance.MusicManager; } }
        public ApplicationThemeManager ApplicationThemeManager { get { return ApplicationThemeManager.Instance; } }
        public RegistryManager RegistryManager { get; set; }

        public ConfigSettings Config
        {
            get { return HurricaneSettings.Instance.Config; }
        }

        public MainWindow BaseWindow
        {
            get { return (MainWindow)Application.Current.MainWindow; }
        }


        #endregion

        private int _selectedtab;
        public int SelectedTab
        {
            get { return _selectedtab; }
            set
            {
                SetProperty(value, ref _selectedtab);
                switch (value)
                {
                    case 2:
                        OnPropertyChanged("MusicManager");
                        break;
                }
            }
        }

        #region Playback

        private List<SoundOutRepresenter> _soundOutList;
        public List<SoundOutRepresenter> SoundOutList
        {
            get { return _soundOutList; }
            set
            {
                SetProperty(value, ref _soundOutList);
            }
        }

        private AudioDevice _selectedaudiodevice;
        public AudioDevice SelectedAudioDevice
        {
            get { return _selectedaudiodevice; }
            set
            {
                if (SetProperty(value, ref _selectedaudiodevice)) OnPropertyChanged("CanApplySoundOut");
            }
        }

        private SoundOutRepresenter _selectedSoundOut;
        public SoundOutRepresenter SelectedSoundOut
        {
            get { return _selectedSoundOut; }
            set
            {
                if (SetProperty(value, ref _selectedSoundOut) && value != null)
                {
                    SelectedAudioDevice = value.AudioDevices.FirstOrDefault(x => x.ID == Config.SoundOutDeviceID) ?? SelectedSoundOut.AudioDevices.First(x => x.IsDefault);
                    OnPropertyChanged("CanApplySoundOut");
                }
            }
        }

        private RelayCommand _applySoundOut;
        public RelayCommand ApplySoundOut
        {
            get
            {
                return _applySoundOut ?? (_applySoundOut = new RelayCommand(parameter =>
                {
                    if (!CanApplySoundOut) return;
                    Config.SoundOutMode = SelectedSoundOut.SoundOutMode;
                    Config.SoundOutDeviceID = SelectedAudioDevice.ID;
                    MusicManager.CSCoreEngine.UpdateSoundOut();
                    OnPropertyChanged("CanApplySoundOut");
                }));
            }
        }

        public bool CanApplySoundOut
        {
            get
            {
                if (SelectedAudioDevice == null || SelectedSoundOut == null) return false;
                return Config.SoundOutDeviceID != SelectedAudioDevice.ID || Config.SoundOutMode != SelectedSoundOut.SoundOutMode;
            }
        }

        #endregion

        #region Apperance

        private RelayCommand _openDesigner;
        public RelayCommand OpenDesigner
        {
            get
            {
                return _openDesigner ?? (_openDesigner = new RelayCommand(parameter =>
                {
                    Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location, "/designer");
                }));
            }
        }

        public bool ShowArtistAndTitle
        {
            get { return Config.ShowArtistAndTitle; }
            set
            {
                Config.ShowArtistAndTitle = value;
                MusicManager.SelectedPlaylist.ViewSource.Refresh();
            }
        }

        public IAppTheme SelectedAppTheme
        {
            get { return Config.ApplicationDesign.AppTheme; }
            set
            {
                Config.ApplicationDesign.AppTheme = value;
                ApplyTheme();
            }
        }

        public IAccentColor SelectedAccentColor
        {
            get { return Config.ApplicationDesign.AccentColor; }
            set
            {
                Config.ApplicationDesign.AccentColor = value;
                ApplyTheme();
            }
        }

        private async void ApplyTheme()
        {
            await BaseWindow.MoveOut();
            ApplicationThemeManager.Instance.Apply(Config.ApplicationDesign);
            await BaseWindow.ResetAndMoveIn();
        }

        private RelayCommand _selectBackground;
        public RelayCommand SelectBackground
        {
            get
            {
                return _selectBackground ?? (_selectBackground = new RelayCommand(async parameter =>
                {
                    var ofd = new OpenFileDialog
                    {
                        Filter = string.Format("{0}|{4};{5}|{1}|{4}|{2}|{5}|{3}|*.*",
                            Application.Current.Resources["SupportedFiles"],
                            Application.Current.Resources["PictureFiles"],
                            Application.Current.Resources["VideoFiles"],
                            Application.Current.Resources["AllFiles"],
                            "*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.gif",
                            "*.mp4;*.wmv")
                    };

                    if (ofd.ShowDialog() == true)
                    {
                        Config.ApplicationDesign.ApplicationBackground = new CustomApplicationBackground { BackgroundPath = ofd.FileName };
                        await BaseWindow.BackgroundChanged();
                    }
                }));
            }
        }

        private RelayCommand _removeBackground;
        public RelayCommand RemoveBackground
        {
            get
            {
                return _removeBackground ?? (_removeBackground = new RelayCommand(async parameter =>
                {
                    Config.ApplicationDesign.ApplicationBackground = null;
                    await BaseWindow.BackgroundChanged();
                }));
            }
        }

        #endregion

        #region App

        public string LocalIPAddress
        {
            get
            {
                return Dns.GetHostAddresses(Dns.GetHostName())
                    .First(a => a.AddressFamily == AddressFamily.InterNetwork).ToString();
            }
        }

        public string AppConnectionString
        {
            get
            {
                return string.Format("{0};{1};{2}", LocalIPAddress, Config.AppCommunicationSettings.Port,
                    Config.AppCommunicationSettings.Password);
            }
        }

        public bool AppIsEnabled
        {
            get { return Config.AppCommunicationSettings.IsEnabled; }
            set
            {
                if (value == Config.AppCommunicationSettings.IsEnabled) return;
                Config.AppCommunicationSettings.IsEnabled = value;
                if (value)
                {
                    try
                    {
                        Config.AppCommunicationManager.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        Config.AppCommunicationSettings.IsEnabled = false;
                    }
                }
                else
                {
                    Config.AppCommunicationManager.Stop();
                }
                OnPropertyChanged();
            }
        }
        

        #endregion

        #region Behaviour

        public bool ShowProgressInTaskbar
        {
            get { return Config.ShowProgressInTaskbar; }
            set
            {
                Config.ShowProgressInTaskbar = value;
                BaseWindow.RefreshTaskbarInfo(MainViewModel.Instance.MusicManager.CSCoreEngine.IsPlaying ? PlaybackState.Playing : PlaybackState.Paused);
            }
        }

        #endregion

        #region Languages

        private LanguageInfo _currentlanguage;
        public LanguageInfo CurrentLanguage
        {
            get { return _currentlanguage; }
            set
            {
                if (SetProperty(value, ref _currentlanguage) && value != null)
                {
                    Config.Language = value.Code;
                    Config.LoadLanguage();
                }
            }
        }

        #endregion

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
                    SelectedAudioDevice = SoundOutList[0].AudioDevices[0];
                    OnPropertyChanged("Config");
                }));
            }
        }
        
        private RelayCommand _selectDownloadPath;
        public RelayCommand SelectDownloadPath
        {
            get
            {
                return _selectDownloadPath ?? (_selectDownloadPath = new RelayCommand(parameter =>
                {
                    var directory = new DirectoryInfo(MusicManager.DownloadManager.DownloadDirectory);
                    var folderBrowserDialog = new WPFFolderBrowserDialog
                    {
                        InitialDirectory = directory.Parent != null ? directory.Parent.FullName : directory.FullName
                    };

                    if (folderBrowserDialog.ShowDialog() == true)
                    {
                        MusicManager.DownloadManager.DownloadDirectory = folderBrowserDialog.FileName;
                    }
                }));
            }
        }
    }
}