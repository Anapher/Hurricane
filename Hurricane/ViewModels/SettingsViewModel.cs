using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Hurricane.Music;
using Hurricane.Music.API;
using Hurricane.Music.Data;
using Hurricane.Settings;
using Hurricane.Settings.RegistryManager;
using Hurricane.Settings.Themes;
using Hurricane.ViewModelBase;
using Hurricane.Views;
using System.Threading.Tasks;
using Hurricane.Settings.MirrorManagement;
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
            RegistryManager = new RegistryManager();
        }

        public void Load()
        {
            Config = ObjectCopier.Clone(HurricaneSettings.Instance.Config);
            SoundOutList = CSCoreEngine.GetSoundOutList();
            SelectedSoundOut = SoundOutList.First(x => x.SoundOutMode == Config.SoundOutMode);
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
                return _applychanges ?? (_applychanges = new RelayCommand(async parameter =>
                {
                    ConfigSettings original = HurricaneSettings.Instance.Config;
                    
                    if (original.Language != Config.Language) { Config.LoadLanguage(); }
                    if (Config.Theme.UseCustomSpectrumAnalyzerColor && string.IsNullOrEmpty(Config.Theme.SpectrumAnalyzerHexColor)) Config.Theme.SpectrumAnalyzerColor = Colors.Black;

                    if (original.ApiIsEnabled != Config.ApiIsEnabled) { if (Config.ApiIsEnabled) { ApiServer.StartListening(); } else { ApiServer.StopListening(); } }

                    bool haveToChangeColorTheme = !original.Theme.SelectedColorTheme.Equals(Config.Theme.SelectedColorTheme);
                    bool haveToChangeBaseTheme = original.Theme.BaseTheme != Config.Theme.BaseTheme;
                    bool haveToRefreshSpectrumAnalyserColor = Config.Theme.SpectrumAnalyzerColor !=
                                                              original.Theme.SpectrumAnalyzerColor;
                    bool haveToUpdateSountOut = Config.SoundOutDeviceID != original.SoundOutDeviceID ||
                                                Config.SoundOutMode != original.SoundOutMode;


                    PropertiesCopier.CopyProperties(Config, original);

                    if (haveToChangeColorTheme || haveToChangeBaseTheme)
                    {
                        var window = Application.Current.MainWindow as MainWindow;
                        if (window != null)
                        {
                            await window.MoveOut();
                            if (haveToChangeColorTheme) { await Task.Run(() => Config.Theme.LoadTheme()); }
                            if (haveToChangeBaseTheme) { await Task.Run(() => Config.Theme.LoadBaseTheme()); }
                            await window.ResetAndMoveIn();
                        }
                    }

                    if (haveToRefreshSpectrumAnalyserColor) original.Theme.RefreshSpectrumAnalyzerBrush();
                    if (haveToUpdateSountOut) MusicManager.CSCoreEngine.UpdateSoundOut();

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
                switch (value)
                {
                    case 2:
                        OnPropertyChanged("MusicManager");
                        break;
                    case 3:
                        OnPropertyChanged("ApiState");
                        break;
                }
            }
        }

        private List<SoundOutRepresenter> _soundOutList;
        public List<SoundOutRepresenter> SoundOutList
        {
            get { return _soundOutList; }
            set
            {
                SetProperty(value, ref _soundOutList);
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

        private SoundOutRepresenter _selectedSoundOut;
        public SoundOutRepresenter SelectedSoundOut
        {
            get { return _selectedSoundOut; }
            set
            {
                if (SetProperty(value, ref _selectedSoundOut) && value != null)
                {
                    Config.SoundOutMode = value.SoundOutMode;
                    OnPropertyChanged("CanApply");
                    SelectedAudioDevice = value.AudioDevices.FirstOrDefault(x => x.ID == Config.SoundOutDeviceID) ?? SelectedSoundOut.AudioDevices.First(x => x.IsDefault);
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
                    SelectedAudioDevice = SoundOutList[0].AudioDevices[0];
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
                    ThemeEditorWindow window = new ThemeEditorWindow() { Owner = Application.Current.MainWindow };
                    if (window.ShowDialog() == true)
                    {
                        var currentThemeIndex = ApplicationThemeManager.Themes.IndexOf(Config.Theme.SelectedColorTheme);
                        ApplicationThemeManager.RefreshThemes();
                        Config.Theme.SelectedColorTheme = ApplicationThemeManager.Themes.Count > currentThemeIndex ? ApplicationThemeManager.Themes[currentThemeIndex] : ApplicationThemeManager.Themes.First();
                        OnPropertyChanged("Config");
                        Debug.Print("test");
                    }
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
                    ThemeEditorWindow window = new ThemeEditorWindow(source) { Owner = Application.Current.MainWindow };
                    window.ShowDialog();
                    if (theme.Name == HurricaneSettings.Instance.Config.Theme.SelectedColorTheme.Name)
                    {
                        theme.RefreshResource();
                        theme.ApplyTheme();
                    }
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
                        StateChanged();
                    }
                }));
            }
        }

        public string ApiState
        {
            get
            {
                if (ApiServer == null) return Application.Current.Resources["Deactivated"].ToString();
                if (ApiServer.IsRunning) { return Application.Current.Resources["Activated"].ToString(); }
                return ApiServer.GotProblems ? Application.Current.Resources["FailedToBindToPort"].ToString() : Application.Current.Resources["Deactivated"].ToString();
            }
        }

        public RegistryManager RegistryManager { get; set; }
    }
}