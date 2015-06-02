using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Hurricane.Model;
using Hurricane.Model.Music;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Notifications;
using Hurricane.Utilities;
using Hurricane.ViewModel.MainView;
using CollectionView = Hurricane.ViewModel.MainView.CollectionView;

namespace Hurricane.ViewModel
{
    public class MainViewModel : PropertyChangedBase
    {
        private IViewItem _selectedViewItem;
        private ObservableCollection<IViewItem> _viewItems;
        private bool _isSettingOpen;
        private RelayCommand _openSettingsCommand;
        private RelayCommand _playPauseCommand;
        private RelayCommand _cancelProgressNotificationCommand;
        private RelayCommand _forwardCommand;
        private RelayCommand _backCommand;

        public MainViewModel()
        {
            var playlist1 = new UserPlaylist {Name = "Beste Musik"};
            _viewItems = new ObservableCollection<IViewItem> {new HomeView {IsPlaying = true}, new CollectionView(), new ChartsView(), new  QueueView(), new PlaylistView(playlist1)};
            
            ViewItems = CollectionViewSource.GetDefaultView(_viewItems);
            ViewItems.GroupDescriptions.Add(new PropertyGroupDescription("ViewCategorie"));
            SelectedViewItem = _viewItems[0];
            MusicDataManager = new MusicDataManager();
            Application.Current.MainWindow.Closing += MainWindow_Closing;
            NotificationManager = new NotificationManager();
            NotificationManager.ShowInformation("HalloWelt", "Es ist ein Fehler aufgetreten.", MessageNotificationIcon.Error);
        }

        public MusicDataManager MusicDataManager { get; }
        public NotificationManager NotificationManager { get; }
        public ICollectionView ViewItems { get; }

        public IViewItem SelectedViewItem
        {
            get { return _selectedViewItem; }
            set
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
            MusicDataManager.Dispose();
        }
    }
}