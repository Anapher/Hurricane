using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Hurricane.Model;
using Hurricane.Model.Music;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Notifications;
using Hurricane.Utilities;
using Hurricane.ViewModel.MainView;

namespace Hurricane.ViewModel
{
    public class MainViewModel : PropertyChangedBase
    {
        private IViewItem _selectedViewItem;
        private bool _isSettingOpen;
        private ViewManager _viewManager;

        private RelayCommand _openSettingsCommand;
        private RelayCommand _playPauseCommand;
        private RelayCommand _cancelProgressNotificationCommand;
        private RelayCommand _forwardCommand;
        private RelayCommand _backCommand;

        public MainViewModel()
        {
            MusicDataManager = new MusicDataManager();
            Application.Current.MainWindow.Closing += MainWindow_Closing;
            NotificationManager = new NotificationManager();
        }

        public MusicDataManager MusicDataManager { get; }
        public NotificationManager NotificationManager { get; }

        public ViewManager ViewManager
        {
            get { return _viewManager; }
            set { SetProperty(value, ref _viewManager); }
        }

        public IViewItem SelectedViewItem
        {
            get { return _selectedViewItem; }
            private set
            {
                if (SetProperty(value, ref _selectedViewItem))
                    value.Load(MusicDataManager, NotificationManager).Forget();
            }
        }

        public bool IsSettingOpen
        {
            get { return _isSettingOpen; }
            set { SetProperty(value, ref _isSettingOpen); }
        }

        public RelayCommand OpenSettingsCommand
        {
            get
            {
                return _openSettingsCommand ?? (_openSettingsCommand = new RelayCommand(parameter =>
                {
                    IsSettingOpen = !IsSettingOpen;
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
                           ((ProgressNotification)parameter).Cancel();
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
            MusicDataManager.Dispose();
        }

        public async Task LoadData()
        {
            try
            {
                var sw = Stopwatch.StartNew();
                await MusicDataManager.Load(AppDomain.CurrentDomain.BaseDirectory);
                Debug.Print($"Dataloading time: {sw.ElapsedMilliseconds}");
            }
            catch (Exception)
            {
                NotificationManager.ShowInformation(Application.Current.Resources["Error"].ToString(),
                    Application.Current.Resources["ErrorWhileLoadingData"].ToString(), MessageNotificationIcon.Error);
            }

            ViewManager = new ViewManager(MusicDataManager.Playlists);
           // MusicDataManager.Playlists.AddPlaylist(new UserPlaylist {Name ="Test"});
        }
    }
}