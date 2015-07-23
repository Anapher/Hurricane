using System.Threading.Tasks;
using System.Windows.Media;
using Hurricane.Model;
using Hurricane.Model.Data;
using Hurricane.Model.Notifications;
using TaskExtensions = Hurricane.Utilities.TaskExtensions;

namespace Hurricane.ViewModel.MainView.Base
{
    public abstract class SideListItem : PropertyChangedBase, ISideListItem
    {
        private bool _isPlaying;

        protected bool IsLoaded;
        protected MusicDataManager MusicDataManager;
        protected ViewController ViewController;
        protected NotificationManager NotificationManager;

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { SetProperty(value, ref _isPlaying); }
        }

        public Task Load(MusicDataManager musicDataManager, ViewController viewController, NotificationManager notificationManager)
        {
            if (IsLoaded)
                return TaskExtensions.CompletedTask;

            MusicDataManager = musicDataManager;
            ViewController = viewController;
            NotificationManager = notificationManager;
            IsLoaded = true;
            return Load();
        }

        public abstract ViewCategorie ViewCategorie { get; }
        public abstract Geometry Icon { get; }
        public abstract string Text { get; }

        protected abstract Task Load();
    }
}