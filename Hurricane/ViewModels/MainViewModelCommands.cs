using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using CSCore.Codecs;
using Hurricane.MagicArrow.DockManager;
using Hurricane.Music;
using Hurricane.Music.Download;
using Hurricane.Music.Playlist;
using Hurricane.Music.Track;
using Hurricane.Settings;
using Hurricane.Utilities;
using Hurricane.ViewModelBase;
using Hurricane.Views;
using Hurricane.Views.MetroDialogs;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace Hurricane.ViewModels
{
    partial class MainViewModel
    {
        private RelayCommand _toggleEqualizer;
        public RelayCommand ToggleEqualizer
        {
            get
            {
                return _toggleEqualizer ?? (_toggleEqualizer = new RelayCommand(parameter =>
                {
                    _baseWindow.ShowEqualizer();
                }));
            }
        }

        private RelayCommand _closeEqualizer;
        public RelayCommand CloseEqualizer
        {
            get
            {
                return _closeEqualizer ?? (_closeEqualizer = new RelayCommand(parameter =>
                {
                    HurricaneSettings.Instance.CurrentState.EqualizerIsOpen = false;
                }));
            }
        }

        private RelayCommand _reloadtrackinformation;
        public RelayCommand ReloadTrackInformation
        {
            get
            {
                return _reloadtrackinformation ?? (_reloadtrackinformation = new RelayCommand(async parameter =>
                {
                    if (!MusicManager.SelectedPlaylist.CanEdit) return;
                    var controller = await _baseWindow.WindowDialogService.CreateProgressDialog(string.Empty, false);

                    await ((NormalPlaylist)MusicManager.SelectedPlaylist).ReloadTrackInformation((s, e) =>
                    {
                        controller.SetProgress(e.Percentage);
                        controller.SetMessage(e.CurrentFile);
                        controller.SetTitle(string.Format(Application.Current.Resources["LoadTrackInformation"].ToString(), e.FilesImported, e.TotalFiles));
                    });

                    MusicManager.SaveToSettings();
                    MySettings.Save();
                    await controller.Close();
                }));
            }
        }

        private RelayCommand _removemissingtracks;
        public RelayCommand RemoveMissingTracks
        {
            get
            {
                return _removemissingtracks ?? (_removemissingtracks = new RelayCommand(async parameter =>
                {
                    if (MusicManager.SelectedPlaylist.CanEdit && await _baseWindow.WindowDialogService.ShowMessage(Application.Current.Resources["DeleteAllMissingTracks"].ToString(), Application.Current.Resources["RemoveMissingTracks"].ToString(), true, DialogMode.Single))
                    {
                        var playlist = (NormalPlaylist)MusicManager.SelectedPlaylist;
                        foreach (var track in playlist.Tracks)
                        {
                            track.IsChecked = false;
                        }
                        AsyncTrackLoader.Instance.RunAsync(playlist);
                    }
                }));
            }
        }

        private RelayCommand _removeduplicatetracks;
        public RelayCommand RemoveDuplicateTracks
        {
            get
            {
                return _removeduplicatetracks ?? (_removeduplicatetracks = new RelayCommand(async parameter =>
                {
                    if (await _baseWindow.WindowDialogService.ShowMessage(Application.Current.Resources["RemoveDuplicateTracks"].ToString(), Application.Current.Resources["RemoveDuplicates"].ToString(), true, DialogMode.First))
                    {
                        var controller = await _baseWindow.WindowDialogService.CreateProgressDialog(Application.Current.Resources["RemoveDuplicates"].ToString(), true);
                        controller.SetMessage(Application.Current.Resources["SearchingForDuplicates"].ToString());

                        var counter = await ((PlaylistBase)MusicManager.SelectedPlaylist).RemoveDuplicates();
                        await controller.Close();
                        await _baseWindow.WindowDialogService.ShowMessage(counter == 0 ? Application.Current.Resources["NoDuplicatesFound"].ToString() : string.Format(Application.Current.Resources["TracksRemoved"].ToString(), counter), Application.Current.Resources["RemoveDuplicates"].ToString(), false, DialogMode.Last);
                    }
                }));
            }
        }

        private RelayCommand _openqueuemanager;
        public RelayCommand OpenQueueManager
        {
            get
            {
                return _openqueuemanager ?? (_openqueuemanager = new RelayCommand(parameter =>
                {
                    _baseWindow.WindowDialogService.ShowDialog(new QueueManagerWindow(MusicManager.Queue));
                }));
            }
        }

        private RelayCommand _addfilestoplaylist;
        public RelayCommand AddFilesToPlaylist
        {
            get
            {
                return _addfilestoplaylist ?? (_addfilestoplaylist = new RelayCommand(async parameter =>
                {
                    if (!MusicManager.SelectedPlaylist.CanEdit) return;

                    var ofd = new OpenFileDialog
                    {
                        CheckFileExists = true,
                        Title = Application.Current.Resources["SelectedFiles"].ToString(),
                        Filter = string.Format("{0}|{1};{2}|{3}|*.*", Application.Current.Resources["SupportedFiles"], GeneralHelper.GetFileDialogFilterFromArray(CodecFactory.Instance.GetSupportedFileExtensions()), GeneralHelper.GetFileDialogFilterFromArray(Playlists.GetSupportedFileExtensions()), Application.Current.Resources["AllFiles"]),
                        Multiselect = true
                    };
                    if (ofd.ShowDialog(_baseWindow) == true)
                        await ImportFiles(ofd.FileNames, (NormalPlaylist)MusicManager.SelectedPlaylist);
                }));
            }
        }

        private RelayCommand _addfoldertoplaylist;
        public RelayCommand AddFolderToPlaylist
        {
            get
            {
                return _addfoldertoplaylist ?? (_addfoldertoplaylist = new RelayCommand(async parameter =>
                {
                    if (!MusicManager.SelectedPlaylist.CanEdit) return;
                    var window = new FolderImportWindow { Owner = _baseWindow };
                    if (window.ShowDialog() != true) return;
                    DirectoryInfo di = new DirectoryInfo(window.SelectedPath);
                    await ImportFiles((from fi in di.GetFiles("*.*", window.IncludeSubfolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly) where LocalTrack.IsSupported(fi) select fi.FullName).ToArray(), (NormalPlaylist)MusicManager.SelectedPlaylist);
                }));
            }
        }

        private RelayCommand _addnewplaylist;
        public RelayCommand AddNewPlaylist
        {
            get
            {
                return _addnewplaylist ?? (_addnewplaylist = new RelayCommand(async parameter =>
                {
                    string result = await _baseWindow.WindowDialogService.ShowInputDialog(Application.Current.Resources["NewPlaylist"].ToString(), Application.Current.Resources["NameOfPlaylist"].ToString(), Application.Current.Resources["Create"].ToString(), string.Empty, DialogMode.Single);
                    if (string.IsNullOrEmpty(result)) return;
                    NormalPlaylist newplaylist = new NormalPlaylist() { Name = result };
                    MusicManager.Playlists.Add(newplaylist);
                    MusicManager.RegisterPlaylist(newplaylist);
                    MusicManager.SelectedPlaylist = newplaylist;
                    MusicManager.SaveToSettings();
                    MySettings.Save();
                }));
            }
        }

        private RelayCommand _removeselectedtracks;
        public RelayCommand RemoveSelectedTracks
        {
            get
            {
                return _removeselectedtracks ?? (_removeselectedtracks = new RelayCommand(async parameter =>
                {
                    if (parameter == null) return;
                    var tracks = ((IList)parameter).Cast<PlayableBase>().ToList();
                    if (tracks.Count == 0) return;
                    if (await _baseWindow.WindowDialogService.ShowMessage(tracks.Count > 1 ? string.Format(Application.Current.Resources["RemoveTracksMessage"].ToString(), tracks.Count) : string.Format(Application.Current.Resources["RemoveTrackMessage"].ToString(), tracks[0].Title), Application.Current.Resources["RemoveTracks"].ToString(), true, DialogMode.Single))
                    {
                        foreach (var t in tracks)
                        {
                            if (t.IsOpened)
                            {
                                MusicManager.CSCoreEngine.StopPlayback();
                                MusicManager.CSCoreEngine.KickTrack();
                            }
                            MusicManager.SelectedPlaylist.RemoveTrack(t);
                        }
                    }
                }));
            }
        }

        private RelayCommand _removeplaylist;
        public RelayCommand RemovePlaylist
        {
            get
            {
                return _removeplaylist ?? (_removeplaylist = new RelayCommand(async parameter =>
                {
                    if (!MusicManager.SelectedPlaylist.CanEdit) return;
                    if (MusicManager.Playlists.Count == 1)
                    {
                        await _baseWindow.WindowDialogService.ShowMessage(Application.Current.Resources["CantDeletePlaylist"].ToString(), Application.Current.Resources["Error"].ToString(), false, DialogMode.Single);
                        return;
                    }
                    if (await _baseWindow.WindowDialogService.ShowMessage(string.Format(Application.Current.Resources["ReallyDeletePlaylist"].ToString(), MusicManager.SelectedPlaylist.Name), Application.Current.Resources["RemovePlaylist"].ToString(), true, DialogMode.Single))
                    {
                        NormalPlaylist playlistToDelete = (NormalPlaylist)MusicManager.SelectedPlaylist;
                        NormalPlaylist newPlaylist = MusicManager.Playlists.First(x => x != playlistToDelete);
                        bool nexttrack = MusicManager.CurrentPlaylist == playlistToDelete;
                        MusicManager.CurrentPlaylist = newPlaylist;
                        if (nexttrack)
                        { MusicManager.CSCoreEngine.StopPlayback(); MusicManager.CSCoreEngine.KickTrack(); MusicManager.GoForward(); }
                        MusicManager.Playlists.Remove(playlistToDelete);
                        MusicManager.SelectedPlaylist = newPlaylist;
                    }
                }));
            }
        }

        private RelayCommand _renameplaylist;
        public RelayCommand RenamePlaylist
        {
            get
            {
                return _renameplaylist ?? (_renameplaylist = new RelayCommand(async parameter =>
                {
                    string result = await _baseWindow.WindowDialogService.ShowInputDialog(Application.Current.Resources["RenamePlaylist"].ToString(), Application.Current.Resources["NameOfPlaylist"].ToString(), Application.Current.Resources["Rename"].ToString(), MusicManager.SelectedPlaylist.Name, DialogMode.Single);
                    if (!string.IsNullOrEmpty(result)) { MusicManager.SelectedPlaylist.Name = result; }
                }));
            }
        }

        private RelayCommand _opentrackinformation;
        public RelayCommand OpenTrackInformation
        {
            get
            {
                return _opentrackinformation ?? (_opentrackinformation = new RelayCommand(parameter =>
                {
                    var track = MusicManager.SelectedTrack;
                    if (track == null || !track.TrackExists) return;
                    _baseWindow.WindowDialogService.ShowDialog(new TrackInformationWindow(MusicManager.SelectedTrack));
                }));
            }
        }

        private RelayCommand _opentageditor;
        public RelayCommand OpenTagEditor
        {
            get
            {
                return _opentageditor ?? (_opentageditor = new RelayCommand(parameter =>
                {
                    var localTrack = MusicManager.SelectedTrack as LocalTrack;
                    if (localTrack == null || !localTrack.TrackExists) return;
                    _baseWindow.WindowDialogService.ShowDialog(new TagEditorWindow(localTrack));
                }));
            }
        }

        private RelayCommand _openupdater;
        public RelayCommand OpenUpdater
        {
            get
            {
                return _openupdater ?? (_openupdater = new RelayCommand(parameter =>
                {
                    _baseWindow.WindowDialogService.ShowDialog(new UpdateWindow(Updater));
                }));
            }
        }

        private RelayCommand _clearselectedplaylist;
        public RelayCommand ClearSelectedPlaylist
        {
            get
            {
                return _clearselectedplaylist ?? (_clearselectedplaylist = new RelayCommand(async parameter =>
                {
                    if (await _baseWindow.WindowDialogService.ShowMessage(string.Format(Application.Current.Resources["RemoveAllTracksQuestion"].ToString(), MusicManager.SelectedPlaylist.Name), Application.Current.Resources["RemoveAllTracks"].ToString(), true, DialogMode.Single))
                    {
                        MusicManager.SelectedPlaylist.Clear();
                    }
                }));
            }
        }

        private RelayCommand _toggleVolume;
        private float _oldVolume;
        public RelayCommand ToggleVolume
        {
            get
            {
                return _toggleVolume ?? (_toggleVolume = new RelayCommand(parameter =>
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (MusicManager.CSCoreEngine.Volume == 0)
                    {
                        MusicManager.CSCoreEngine.Volume = _oldVolume;
                    }
                    else
                    {
                        _oldVolume = MusicManager.CSCoreEngine.Volume;
                        MusicManager.CSCoreEngine.Volume = 0;
                    }
                }));
            }
        }

        private RelayCommand _openOnlineSection;
        public RelayCommand OpenOnlineSection
        {
            get
            {
                return _openOnlineSection ?? (_openOnlineSection = new RelayCommand(parameter =>
                {
                    MainTabControlIndex = 3;
                }));
            }
        }

        private RelayCommand _openDownloadManager;
        public RelayCommand OpenDownloadManager
        {
            get
            {
                return _openDownloadManager ?? (_openDownloadManager = new RelayCommand(parameter =>
                {
                    MusicManager.DownloadManager.IsOpen = true;
                }));
            }
        }

        private RelayCommand _convertStreamToLocalTrack;
        public RelayCommand ConvertStreamToLocalTrack
        {
            get
            {
                return _convertStreamToLocalTrack ?? (_convertStreamToLocalTrack = new RelayCommand(async parameter =>
                {
                    var track = MusicManager.SelectedTrack as StreamableBase;
                    if (track == null) return;
                    if (!track.CanDownload) return;

                    var downloadDialog = new DownloadTrackWindow(track.DownloadFilename, DownloadManager.GetExtension(track)) { Owner = _baseWindow };
                    if (downloadDialog.ShowDialog() == true)
                    {
                        var controller = await _baseWindow.ShowProgressAsync(Application.Current.Resources["Download"].ToString(), MusicManager.SelectedTrack.Title);

                        if (await
                            DownloadManager.DownloadAndConfigureTrack(track, track, downloadDialog.SelectedPath,
                                d =>
                                {
                                    controller.SetProgress(d / 100);
                                }, downloadDialog.DownloadSettings.Clone()))
                        {
                            var newTrack = new LocalTrack { Path = downloadDialog.SelectedPath };
                            if (await newTrack.LoadInformation())
                            {
                                newTrack.TimeAdded = track.TimeAdded;
                                newTrack.Artist = track.Artist;
                                newTrack.Year = track.Year;
                                newTrack.Title = track.Title;
                                newTrack.Album = track.Album;
                                newTrack.Genres = track.Genres;

                                if (MusicManager.FavoriteListIsSelected)
                                {
                                    foreach (var normalPlaylist in MusicManager.Playlists)
                                    {
                                        if (normalPlaylist.Tracks.Contains(track))
                                        {
                                            normalPlaylist.Tracks[normalPlaylist.Tracks.IndexOf(track)] = newTrack;
                                        }
                                    }
                                    track.IsFavorite = false; //To remove from the favorite list
                                }
                                else
                                {
                                    MusicManager.SelectedPlaylist.Tracks[
                                        MusicManager.SelectedPlaylist.Tracks.IndexOf(track)] = newTrack;
                                }

                                newTrack.IsFavorite = track.IsFavorite;
                                if (track.IsOpened)
                                    MusicManager.PlayTrack(newTrack, MusicManager.SelectedPlaylist);

                                await controller.CloseAsync();
                            }
                            else
                            {
                                await controller.CloseAsync();
                                await
                                    _baseWindow.WindowDialogService.ShowMessage(
                                        Application.Current.Resources["ExceptionConvertTrack"].ToString(),
                                        Application.Current.Resources["Exception"].ToString(), false, DialogMode.Single);
                            }
                        }
                        else
                        {
                            await controller.CloseAsync();
                            await _baseWindow.WindowDialogService.ShowMessage(Application.Current.Resources["ExceptionConvertTrack"].ToString(), Application.Current.Resources["Exception"].ToString(), false, DialogMode.Single);
                        }
                    }
                }));
            }
        }

        private RelayCommand _downloadAllStreams;
        public RelayCommand DownloadAllStreams
        {
            get
            {
                return _downloadAllStreams ?? (_downloadAllStreams = new RelayCommand(async parameter =>
                {
                    if (MusicManager.FavoriteListIsSelected) return;
                    var lst = MusicManager.SelectedPlaylist.Tracks.OfType<StreamableBase>().Where(x => x.CanDownload).ToList();
                    if (!lst.Any()) return;

                    var downloadDialog = new DownloadTrackWindow { Owner = _baseWindow };
                    if (downloadDialog.ShowDialog() == true)
                    {
                        var downloadSettings = downloadDialog.DownloadSettings.Clone();
                        var controller = await _baseWindow.ShowProgressAsync(Application.Current.Resources["Download"].ToString(), "", true, new MetroDialogSettings { NegativeButtonText = Application.Current.Resources["Cancel"].ToString() });
                        foreach (var track in lst)
                        {
                            if (controller.IsCanceled)
                            {
                                await controller.CloseAsync();
                                return;
                            }
                            controller.SetMessage(string.Format(Application.Current.Resources["TrackIsDownloading"].ToString(), track.Title));
                            var downloadFile = new FileInfo(Path.Combine(downloadDialog.SelectedPath, track.DownloadFilename + downloadSettings.GetExtension(track)));
                            if (downloadFile.Exists) continue;
                            var staticTrack = track;
                            if (await
                                DownloadManager.DownloadAndConfigureTrack(track, track, downloadFile.FullName,
                                    d =>
                                    {
                                        controller.SetProgress(lst.IndexOf(staticTrack) / (double)lst.Count +
                                                               1 / (double)lst.Count / 100 * d);
                                    }, downloadSettings))
                            {
                                var newTrack = new LocalTrack { Path = downloadFile.FullName };
                                if (await newTrack.LoadInformation())
                                {
                                    newTrack.TimeAdded = track.TimeAdded;
                                    newTrack.Artist = track.Artist;
                                    newTrack.Year = track.Year;
                                    newTrack.Title = track.Title;
                                    newTrack.Album = track.Album;
                                    newTrack.Genres = track.Genres;
                                    MusicManager.SelectedPlaylist.Tracks[MusicManager.SelectedPlaylist.Tracks.IndexOf(track)] = newTrack;
                                }
                            }
                        }

                        MusicManager.SaveToSettings();
                        HurricaneSettings.Instance.Save();
                        await controller.CloseAsync();
                    }
                }));
            }
        }

        private RelayCommand _addCustomStream;
        public RelayCommand AddCustomStream
        {
            get
            {
                return _addCustomStream ?? (_addCustomStream = new RelayCommand(async parameter =>
                {
                    var dialog = new AddCustomStreamView((NormalPlaylist)MusicManager.SelectedPlaylist, MusicManager, x => _baseWindow.HideMetroDialogAsync(x));
                    await _baseWindow.ShowMetroDialogAsync(dialog);
                }));
            }
        }

        private RelayCommand _moveLeftCommand;
        public RelayCommand MoveLeftCommand
        {
            get
            {
                return _moveLeftCommand ?? (_moveLeftCommand = new RelayCommand(parameter =>
                {
                    MoveWindow(true);
                }));
            }
        }

        private RelayCommand _moveRightCommand;
        public RelayCommand MoveRightCommand
        {
            get
            {
                return _moveRightCommand ?? (_moveRightCommand = new RelayCommand(parameter =>
                {
                    MoveWindow(false);
                }));
            }
        }

        private void MoveWindow(bool moveLeft)
        {
            var dockmanager = _baseWindow.MagicArrow.DockManager;
            if (dockmanager.CurrentSide == DockingSide.None)
            {
                dockmanager.CurrentSide = moveLeft ? DockingSide.Left : DockingSide.Right;
                dockmanager.ApplyCurrentSide();
                _baseWindow.RefreshHostWindow(true);
            }
            if (dockmanager.CurrentSide == (moveLeft ? DockingSide.Right : DockingSide.Left))
            {
                dockmanager.CurrentSide = DockingSide.None;
                dockmanager.ApplyCurrentSide();
                _baseWindow.RefreshHostWindow(true);
                _baseWindow.CenterWindowOnScreen();
            }
        }

        private RelayCommand _changeMainTabControlIndex;
        public RelayCommand ChangeMainTabControlIndex
        {
            get
            {
                return _changeMainTabControlIndex ?? (_changeMainTabControlIndex = new RelayCommand(parameter =>
                {
                    MainTabControlIndex = int.Parse(parameter.ToString());
                }));
            }
        }

        private RelayCommand _showWindoCommand;
        public RelayCommand ShowWindowCommand
        {
            get { return _showWindoCommand ?? (_showWindoCommand = new RelayCommand(parameter => { _baseWindow.ShowWindow(); })); }
        }

        private RelayCommand _openFileLocation;
        public RelayCommand OpenFileLocation
        {
            get
            {
                return _openFileLocation ?? (_openFileLocation = new RelayCommand(parameter =>
                {
                    if (parameter == null || string.IsNullOrEmpty(parameter.ToString())) return;
                    var file = new FileInfo(parameter.ToString());
                    if (!file.Exists) return;
                    Process.Start("explorer.exe", string.Format("/select,\"{0}\"", file.FullName));
                }));
            }
        }
    }
}
