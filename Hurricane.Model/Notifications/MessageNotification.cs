using System;
using System.Windows.Threading;

namespace Hurricane.Model.Notifications
{
    public class MessageNotification : INotificationItem
    {
        public MessageNotification(string title, string message, MessageNotificationIcon icon, TimeSpan timeStayOpen)
        {
            Title = title;
            Message = message;
            Icon = icon;
            var timer = new DispatcherTimer {Interval = timeStayOpen};
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public event EventHandler Close;

        public string Title { get; }
        public MessageNotificationIcon Icon { get; }
        public string Message { get; }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ((DispatcherTimer) sender).Stop();
            Close?.Invoke(this, EventArgs.Empty);
        }
    }

    public enum MessageNotificationIcon
    {
        Information,
        Error
    }
}