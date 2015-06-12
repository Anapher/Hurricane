using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Hurricane.Model;
using Hurricane.Model.Data;
using Hurricane.Model.Music;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Notifications;
using Hurricane.Utilities;
using Ookii.Dialogs.Wpf;
using TaskExtensions = Hurricane.Utilities.TaskExtensions;

namespace Hurricane.ViewModel.MainView
{
    public class CollectionView : PropertyChangedBase, IViewItem
    {
        private string _searchText;
        private bool _isPlaying;
        private bool _isLoaded;
        private NotificationManager _notificationManager;
        private MusicDataManager _musicDataManager;

        private RelayCommand _addFilesCommand;
        private RelayCommand _addDirectoryCommand;
        private RelayCommand _playAudioCommand;

        public ICollectionView ViewSource { get; private set; }
        public ViewCategorie ViewCategorie { get; } = ViewCategorie.MyMusic;
        public ICollectionView AlbumViewSource { get; set; }

        public string Text => Application.Current.Resources["Collection"].ToString();
        public Geometry Icon { get; } = (Geometry)Application.Current.Resources["VectorCollection"];
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { SetProperty(value, ref _isPlaying); }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SetProperty(value, ref _searchText))
                    ViewSource.Refresh();
            }
        }
        
        public RelayCommand AddFilesCommand
        {
            get
            {
                return _addFilesCommand ?? (_addFilesCommand = new RelayCommand(async parameter =>
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append($"{Application.Current.Resources["AudioFiles"]}|");
                    stringBuilder.Append(string.Concat(_musicDataManager.MusicManager.AudioEngine.SupportedExtensions.Select(x => "*." + x + ";").ToArray()));
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                    stringBuilder.Append($"|{Application.Current.Resources["AllFiles"]}|*.*");

                    var ofd = new VistaOpenFileDialog
                    {
                        Multiselect = true,
                        Title = Application.Current.Resources["AddFilesOpenFileDialogTitle"].ToString(),
                        CheckFileExists = true,
                        Filter = stringBuilder.ToString(),
                        CheckPathExists = true,
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
                    };

                    if (ofd.ShowDialog(Application.Current.MainWindow) == true)
                    {
                        var importer = new TrackImporter(_musicDataManager);
                        _notificationManager.ShowProgress(Application.Current.Resources["ImportingTracks"].ToString(), importer);
                        await importer.ImportTracks(ofd.FileNames.Select(x => new FileInfo(x)));
                    }
                }));
            }
        }

        public RelayCommand AddDirectoryCommand
        {
            get
            {
                return _addDirectoryCommand ?? (_addDirectoryCommand = new RelayCommand(async parameter =>
                {
                    var fbd = new VistaFolderBrowserDialog
                    {
                        ShowNewFolderButton = false,
                        RootFolder = Environment.SpecialFolder.MyMusic
                    };
                    if (fbd.ShowDialog(Application.Current.MainWindow) == true)
                    {
                        var importer = new TrackImporter(_musicDataManager);
                        _notificationManager.ShowProgress(Application.Current.Resources["ImportingTracks"].ToString(), importer);
                        await importer.ImportDirectory(new DirectoryInfo(fbd.SelectedPath), true);
                    }
                }));
            }
        }

        public RelayCommand PlayAudioCommand
        {
            get
            {
                return _playAudioCommand ?? (_playAudioCommand = new RelayCommand(parameter =>
                {
                    var playable = parameter as PlayableBase;
                    if (playable == null)
                        return;

                    _musicDataManager.MusicManager.OpenPlayable(playable, _musicDataManager.Tracks).Forget();
                }));
            }
        }

        public Task Load(MusicDataManager musicDataManager, ViewController viewController, NotificationManager notificationManager)
        {
            if (!_isLoaded)
            {
                ViewSource = CollectionViewSource.GetDefaultView(musicDataManager.Tracks.Tracks);
                ViewSource.Filter = FilterViewSource;
                ViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Artist"));
                ViewSource.SortDescriptions.Add(new SortDescription("Artist.Name", ListSortDirection.Ascending));

                AlbumViewSource = new CollectionViewSource {Source = musicDataManager.Tracks.Tracks}.View;
                AlbumViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Album"));
                AlbumViewSource.SortDescriptions.Add(new SortDescription("Album.Name", ListSortDirection.Ascending));

                _notificationManager = notificationManager;
                _musicDataManager = musicDataManager;
                _isLoaded = true;
            }

            return TaskExtensions.CompletedTask;
        }

        private bool FilterViewSource(object o)
        {
            var track = (IPlayable)o;
            return string.IsNullOrWhiteSpace(_searchText) ||
                   track.Title.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) > -1 ||
                   track.Artist?.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) > -1;
        }
    }
}