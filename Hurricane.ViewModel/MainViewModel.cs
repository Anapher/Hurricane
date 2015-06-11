using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Hurricane.Model;
using Hurricane.Model.AudioEngine;
using Hurricane.Model.Music;
using Hurricane.Model.Notifications;
using Hurricane.Model.Settings;
using Hurricane.Utilities;
using Hurricane.ViewModel.MainView;

namespace Hurricane.ViewModel
{
    public class MainViewModel : PropertyChangedBase
    {
        private IViewItem _selectedViewItem;
        private ViewManager _viewManager;
        private int _currentMainView = 1;

        private RelayCommand _openSettingsCommand;
        private RelayCommand _playPauseCommand;
        private RelayCommand _cancelProgressNotificationCommand;
        private RelayCommand _forwardCommand;
        private RelayCommand _backCommand;

        public MainViewModel()
        {
            MusicDataManager = new MusicDataManager();
            MusicDataManager.MusicManager.AudioEngine.ErrorOccurred += AudioEngine_ErrorOccurred;
            Application.Current.MainWindow.Closing += MainWindow_Closing;
            NotificationManager = new NotificationManager();
        }

        public event EventHandler RefreshView;

        public MusicDataManager MusicDataManager { get; }
        public NotificationManager NotificationManager { get; }
        public SettingsViewModel SettingsViewModel { get; private set; }
        public SettingsData Settings { get; } = SettingsManager.Current;

        public ViewManager ViewManager
        {
            get { return _viewManager; }
            set { SetProperty(value, ref _viewManager); }
        }

        public IViewItem SelectedViewItem
        {
            get { return _selectedViewItem; }
            protected set
            {
                if (SetProperty(value, ref _selectedViewItem))
                    value.Load(MusicDataManager, NotificationManager).Forget();
            }
        }

        public int CurrentMainView
        {
            get { return _currentMainView; }
            set { SetProperty(value, ref _currentMainView); }
        }

        public RelayCommand OpenSettingsCommand
        {
            get
            {
                return _openSettingsCommand ?? (_openSettingsCommand = new RelayCommand(parameter =>
                {
                    CurrentMainView = CurrentMainView == 0 ? 1 : 0;
                }));
            }
        }

        public RelayCommand PlayPauseCommand
        {
            get
            {
                return _playPauseCommand ?? (_playPauseCommand = new RelayCommand(parameter =>
                {
                    MusicDataManager.MusicManager.AudioEngine.TogglePlayPause();
                }));
            }
        }

        public RelayCommand CancelProgressNotificationCommand
        {
            get
            {
                return _cancelProgressNotificationCommand ??
                       (_cancelProgressNotificationCommand = new RelayCommand(parameter =>
                       {
                           ((ProgressNotification) parameter).Cancel();
                       }));
            }
        }

        public RelayCommand ForwardCommand
        {
            get { return _forwardCommand ?? (_forwardCommand = new RelayCommand(parameter => { MusicDataManager.MusicManager.GoForward(); })); }
        }

        public RelayCommand BackCommand
        {
            get { return _backCommand ?? (_backCommand = new RelayCommand(parameter => { MusicDataManager.MusicManager.GoBack(); })); }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            MusicDataManager.Save(AppDomain.CurrentDomain.BaseDirectory);
            SettingsManager.Save("settings.xml");
            MusicDataManager.Dispose();
        }

        public async Task LoadData()
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var settingsFile = new FileInfo("settings.xml");
                if (settingsFile.Exists)
                    SettingsManager.Load(settingsFile.FullName);
                else SettingsManager.InitalizeNew();
                
                await MusicDataManager.Load(AppDomain.CurrentDomain.BaseDirectory);
                Debug.Print($"Dataloading time: {sw.ElapsedMilliseconds}");
                SettingsViewModel = new SettingsViewModel(MusicDataManager, () => RefreshView?.Invoke(this, EventArgs.Empty));
            }
            catch (Exception ex)
            {
                NotificationManager.ShowInformation(Application.Current.Resources["Error"].ToString(),
                    Application.Current.Resources["ErrorWhileLoadingData"].ToString(), MessageNotificationIcon.Error);
            }

            ViewManager = new ViewManager(MusicDataManager.Playlists);
        }

        private void AudioEngine_ErrorOccurred(object sender, ErrorOccurredEventArgs e)
        {
            NotificationManager.ShowInformation(Application.Current.Resources["PlaybackError"].ToString(), e.ErrorMessage, MessageNotificationIcon.Error);
        }
    }
}