using System.ComponentModel;
using System.Threading.Tasks;
using Hurricane.Model.Data;
using Hurricane.Model.Notifications;

namespace Hurricane.ViewModel.MainView.Base
{
    public interface IViewItem : INotifyPropertyChanged
    {
        bool IsPlaying { get; set; }
        Task Load(MusicDataManager musicDataManager, ViewController viewController,
            NotificationManager notificationManager);
    }
}