using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Hurricane.Model;
using Hurricane.Model.Music;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using Hurricane.Utilities;
using Hurricane.ViewModel.MainView;
using CollectionView = Hurricane.ViewModel.MainView.CollectionView;

namespace Hurricane.ViewModel
{
    public class NormalViewModel : PropertyChangedBase
    {
        private IViewItem _selectedViewItem;
        private ObservableCollection<IViewItem> _viewItems;
        private bool _isSettingOpen;
        private RelayCommand _openSettingsCommand;
        private RelayCommand _playPauseCommand;

        public NormalViewModel()
        {
            var playlist1 = new UserPlaylist {Name = "Beste Musik eva & adam"};
            playlist1.Tracks.Add(new LocalPlayable
            {
                Artist = "eminem",
                TrackPath = @"D:\Musik\Best\Moderat - Bad.mp3",
                Title = "Moderat - Bad",
                Duration = TimeSpan.FromSeconds(193),
                Album = "Die Garcons 2"
            });
            _viewItems = new ObservableCollection<IViewItem> {new HomeView {IsPlaying = true}, new CollectionView(), new ChartsView(), new  QueueView(), new PlaylistView(playlist1)};
            
            ViewItems = CollectionViewSource.GetDefaultView(_viewItems);
            ViewItems.GroupDescriptions.Add(new PropertyGroupDescription("ViewCategorie"));
            SelectedViewItem = _viewItems[0];
            MusicDataManager = new MusicDataManager();
        }

        public MusicDataManager MusicDataManager { get; }

        public IViewItem SelectedViewItem
        {
            get { return _selectedViewItem; }
            set
            {
                if (SetProperty(value, ref _selectedViewItem))
                    value.Load(MusicDataManager).Forget();
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

        public ICollectionView ViewItems { get; set; }
    }
}