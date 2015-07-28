using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Hurricane.Model.Data;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.Model.Notifications;
using Hurricane.Utilities;
using Hurricane.ViewModel.MainView.Base;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Ookii.Dialogs.Wpf;
using TaskExtensions = Hurricane.Utilities.TaskExtensions;

namespace Hurricane.ViewModel.MainView
{
    public class CollectionView : SideListItem
    {
        private string _searchText;
        private NotificationManager _notificationManager;

        private RelayCommand _addFilesCommand;
        private RelayCommand _addDirectoryCommand;
        private RelayCommand _playAudioCommand;
        private RelayCommand _addToQueueCommand;
        private RelayCommand _openArtistCommand;
        private RelayCommand _openLocalTrackLocationCommand;
        private RelayCommand _removePlayablesCommand;

        public ICollectionView ViewSource { get; private set; }
        public override ViewCategorie ViewCategorie { get; } = ViewCategorie.MyMusic;
        public ICollectionView AlbumViewSource { get; set; }
        public ObservableCollection<PlayableBase> Tracks { get; set; }

        public override string Text => Application.Current.Resources["Collection"].ToString();

        public override Geometry Icon { get; } = (Geometry)Application.Current.Resources["VectorCollection"];

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
                        var importer = new TrackImporter(MusicDataManager);
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

                    MusicDataManager.MusicManager.OpenPlayable(playable, MusicDataManager.Tracks).Forget();
                    ViewController.SetIsPlaying(this);
                }));
            }
        }

        public RelayCommand AddToQueueCommand
        {
            get
            {
                return _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand(parameter =>
                {
                    var list = parameter as IList;
                    if (list != null)
                    {
                        var items = list;

                        foreach (var track in items.Cast<PlayableBase>())
                        {
                            if (track.IsQueued)
                                MusicDataManager.MusicManager.Queue.RemoveTrackFromQueue(track);
                            else
                                MusicDataManager.MusicManager.Queue.AddTrackToQueue(track);
                        }

                        return;
                    }

                    var playable = parameter as PlayableBase;
                    if (playable != null)
                    {
                        if (playable.IsQueued)
                            MusicDataManager.MusicManager.Queue.RemoveTrackFromQueue(playable);
                        else
                            MusicDataManager.MusicManager.Queue.AddTrackToQueue(playable);
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

        public RelayCommand OpenLocalTrackLocationCommand
        {
            get
            {
                return _openLocalTrackLocationCommand ?? (_openLocalTrackLocationCommand = new RelayCommand(parameter =>
                {
                    var localTrack = parameter as LocalPlayable;
                    if (localTrack != null)
                    {
                        Process.Start("explorer.exe", $"/select,\"{localTrack.TrackPath}\"");
                        return;
                    }

                    var streamable = parameter as Streamable;
                    if (streamable != null)
                        Process.Start(streamable.Url);
                }));
            }
        }

        public RelayCommand RemovePlayablesCommand
        {
            get
            {
                return _removePlayablesCommand ?? (_removePlayablesCommand = new RelayCommand(async parameter =>
                {
                    var playables = parameter as IList;
                    if (playables == null)
                        return;

                    var list = playables.Cast<PlayableBase>().ToList();
                    if (
                        await
                            ((MetroWindow) Application.Current.MainWindow).ShowMessageAsync(
                                Application.Current.Resources["RemoveTracks"].ToString(),
                                list.Count == 1
                                    ? string.Format(Application.Current.Resources["RemoveTrackMessage"].ToString(),
                                        list[0].Title)
                                    : string.Format(Application.Current.Resources["RemoveTracksMessage"].ToString(),
                                        list.Count),
                                MessageDialogStyle.AffirmativeAndNegative,
                                new MetroDialogSettings
                                {
                                    AffirmativeButtonText = Application.Current.Resources["Remove"].ToString(),
                                    NegativeButtonText = Application.Current.Resources["Cancel"].ToString()
                                }) !=
                        MessageDialogResult.Affirmative)
                        return;

                    var goForward = false;
                    foreach (var playableBase in list)
                    {
                        foreach (var playlist in MusicDataManager.Playlists.Playlists)
                        {
                            //Check for playlists
                            if (playlist.Tracks.Contains(playableBase))
                                playlist.RemoveTrack(playableBase);

                            //Check queue
                            if (MusicDataManager.MusicManager.Queue.QueueItems.Any(x => x.Playable == playableBase))
                                MusicDataManager.MusicManager.Queue.RemoveTrackFromQueue(playableBase);

                            //Check if track is playing
                            if (MusicDataManager.MusicManager.CurrentTrack == playableBase)
                                goForward = true;

                            //Good bye
                            await MusicDataManager.Tracks.RemoveTrack(playableBase);
                        }
                    }

                    if (goForward)
                        await MusicDataManager.MusicManager.GoForward();
                }));
            }
        }

        protected override Task Load()
        {
            ViewSource = CollectionViewSource.GetDefaultView(MusicDataManager.Tracks.Tracks);
            ViewSource.Filter = FilterViewSource;
            ViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Artist"));
            ViewSource.SortDescriptions.Add(new SortDescription("Artist.Name", ListSortDirection.Ascending));

            AlbumViewSource = new CollectionViewSource { Source = MusicDataManager.Tracks.Tracks }.View;
            AlbumViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Album"));
            AlbumViewSource.SortDescriptions.Add(new SortDescription("Album.Name", ListSortDirection.Ascending));

            Tracks = MusicDataManager.Tracks.Tracks;

            _notificationManager = NotificationManager;
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