using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.Utilities;
using Hurricane.ViewModel.MainView.Base;
using Queue = Hurricane.Model.Music.Playlist.Queue;
using TaskExtensions = Hurricane.Utilities.TaskExtensions;

namespace Hurricane.ViewModel.MainView
{
    public class QueueView : SideListItem
    {
        private RelayCommand _playAudioCommand;
        private RelayCommand _openArtistCommand;
        private RelayCommand _clearQueueCommand;
        private RelayCommand _removeFromQueueCommand;

        public override ViewCategorie ViewCategorie { get; } = ViewCategorie.Discover;
        public override Geometry Icon { get; } = (Geometry)Application.Current.Resources["VectorQueue"];
        public override string Text => Application.Current.Resources["Queue"].ToString();

        public Queue Queue { get; private set; }

        public RelayCommand PlayAudioCommand
        {
            get
            {
                return _playAudioCommand ?? (_playAudioCommand = new RelayCommand(parameter =>
                {
                    var queueItem = parameter as QueueItem;
                    if (queueItem == null)
                        return;

                    MusicDataManager.MusicManager.OpenPlayable(queueItem.Playable, null).Forget();
                    Queue.RemoveTrackFromQueue(queueItem.Playable);
                    ViewController.SetIsPlaying(this);
                }));
            }
        }

        public RelayCommand OpenArtistCommand
        {
            get
            {
                return _openArtistCommand ?? (_openArtistCommand = new RelayCommand(parameter =>
                {
                    var artist = parameter as Artist;

                    if (artist == null)
                    {
                        var artistName = parameter as string;
                        if (string.IsNullOrWhiteSpace(artistName))
                            return;

                        var artistMatches =
                            MusicDataManager.Artists.ArtistDictionary.Where(x => string.Equals(x.Value.Name, artistName, StringComparison.OrdinalIgnoreCase)).ToList();
                        if (!artistMatches.Any())
                            return;
                        artist = artistMatches.First().Value;
                    }

                    ViewController.OpenArtist(artist);
                }));
            }
        }

        public RelayCommand ClearQueue
        {
            get
            {
                return _clearQueueCommand ?? (_clearQueueCommand = new RelayCommand(parameter =>
                {
                    Queue.QueueItems.Clear();
                }));
            }
        }

        public RelayCommand RemoveFromQueueCommand
        {
            get
            {
                return _removeFromQueueCommand ?? (_removeFromQueueCommand = new RelayCommand(parameter =>
                {
                    var items = parameter as IList;
                    if (items == null)
                        return;

                    foreach (var queueItem in items.Cast<QueueItem>())
                        Queue.QueueItems.Remove(queueItem);
                }));
            }
        }

        protected override Task Load()
        {
            Queue = MusicDataManager.MusicManager.Queue;
            MusicDataManager.MusicManager.QueuePlaying += MusicManager_QueuePlaying;

            return TaskExtensions.CompletedTask;
        }

        private void MusicManager_QueuePlaying(object sender, EventArgs e)
        {
            ViewController?.SetIsPlaying(this);
        }
    }
}