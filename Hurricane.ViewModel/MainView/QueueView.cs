using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.Music.Playlist;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.Utilities;
using Hurricane.ViewModel.MainView.Base;
using TaskExtensions = Hurricane.Utilities.TaskExtensions;

namespace Hurricane.ViewModel.MainView
{
    public class QueueView : SideListItem
    {
        private RelayCommand _playAudioCommand;
        private RelayCommand _openArtistCommand;

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
                    var playable = parameter as IPlayable;
                    if (playable == null)
                        return;

                    MusicDataManager.MusicManager.OpenPlayable(playable, null).Forget();
                    Queue.Playables.Remove(playable);
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
                    ViewController.OpenArtist((Artist)parameter);
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