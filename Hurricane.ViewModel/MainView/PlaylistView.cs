using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Hurricane.Model.Data;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.Utilities;
using Hurricane.ViewModel.MainView.Base;
using Ookii.Dialogs.Wpf;
using TaskExtensions = Hurricane.Utilities.TaskExtensions;

namespace Hurricane.ViewModel.MainView
{
    public class PlaylistView : SideListItem
    {
        private string _searchText;

        private RelayCommand _addFilesCommand;
        private RelayCommand _addDirectoryCommand;
        private RelayCommand _playAudioCommand;
        private RelayCommand _openArtistCommand;
        private RelayCommand _addToQueueCommand;

        public PlaylistView(UserPlaylist playlist)
        {
            Playlist = playlist;
            Icon = (Geometry)Application.Current.Resources["VectorPlaylist"];
        }

        public override ViewCategorie ViewCategorie { get; } = ViewCategorie.Playlist;
        public override Geometry Icon { get; }
        public override string Text => Playlist.Name;

        public UserPlaylist Playlist { get; }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SetProperty(value, ref _searchText))
                    ViewSource.Refresh();
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

                    MusicDataManager.MusicManager.OpenPlayable(playable, Playlist).Forget();
                    ViewController.SetIsPlaying(this);
                }));
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
                    stringBuilder.Append(string.Concat(MusicDataManager.MusicManager.AudioEngine.SupportedExtensions.Select(x => "*." + x + ";").ToArray()));
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
                        var importer = new TrackImporter(MusicDataManager);
                        NotificationManager.ShowProgress(Application.Current.Resources["ImportingTracks"].ToString(), importer);
                        await importer.ImportTracks(ofd.FileNames.Select(x => new FileInfo(x)), Playlist);
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
                        var importer = new TrackImporter(MusicDataManager);
                        NotificationManager.ShowProgress(Application.Current.Resources["ImportingTracks"].ToString(), importer);
                        await importer.ImportDirectory(new DirectoryInfo(fbd.SelectedPath), true, Playlist);
                    }
                }));
            }
        }

        public RelayCommand OpenArtistCommand
        {
            get
            {
                return _openArtistCommand ?? (_openArtistCommand = new RelayCommand(parameter =>
                {
                    ViewController.OpenArtist((Artist)parameter);
                }));
            }
        }

        public RelayCommand AddToQueueCommand
        {
            get
            {
                return _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand(parameter =>
                {
                    var items = parameter as IList;
                    if (items == null)
                        return;

                    foreach (var track in items.Cast<PlayableBase>())
                    {
                        MusicDataManager.MusicManager.Queue.AddTrackToQueue(track);
                    }
                }));
            }
        }

        public ICollectionView ViewSource { get; private set; }

        protected override Task Load()
        {
            ViewSource = CollectionViewSource.GetDefaultView(Playlist.Tracks);
            ViewSource.Filter = FilterViewSource;
            return TaskExtensions.CompletedTask;
        }

        private bool FilterViewSource(object o)
        {
            var track = (IPlayable)o;
            if (o == null)
                return false;

            return string.IsNullOrWhiteSpace(_searchText) ||
                   track.Title.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) > -1 ||
                   track.Artist?.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) > -1;
        }
    }
}