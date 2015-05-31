using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Hurricane.Model.Notifications
{
    public class NotificationManager : INotifyPropertyChanged
    {
        private bool _isVisible;

        public NotificationManager()
        {
            Notifications = new ObservableCollection<INotificationItem>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<INotificationItem> Notifications { get; }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public void ShowInformation(string title, string message, MessageNotificationIcon icon)
        {
            var messageNotification = new MessageNotification(title, message, icon, TimeSpan.FromSeconds(4));
            messageNotification.Close += MessageNotification_Close;
            Notifications.Add(messageNotification);
            IsVisible = true;
        }

        public void ShowProgress(string title, IProgressReporter progressReporter)
        {
            var progressNotification = new ProgressNotification(title, progressReporter);
            progressNotification.Close += MessageNotification_Close;
            Notifications.Add(progressNotification);
            IsVisible = true;
        }

        private void MessageNotification_Close(object sender, EventArgs e)
        {
            var notification = (INotificationItem) sender;
            Notifications.Remove(notification);
            if (!Notifications.Any())
                IsVisible = false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}