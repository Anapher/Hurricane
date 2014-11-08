using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Notification
{
    class NotificationService
    {
        public NotificationService(Music.CSCore cscore)
        {
            cscore.TrackChanged += cscore_TrackChanged;
        }

        void cscore_TrackChanged(object sender, Music.TrackChangedEventArgs e)
        {
            TrackChanged(e.NewTrack);
        }

        private System.Windows.Window lastwindow;
        void TrackChanged(Music.Track newtrack)
        {
            Settings.ConfigSettings config = Settings.HurricaneSettings.Instance.Config;
            if (config.Notification == NotificationType.None) return;
            if (lastwindow != null && lastwindow.Visibility == System.Windows.Visibility.Visible) lastwindow.Close();
            TimeSpan timetostayopen = TimeSpan.FromMilliseconds(5000);
            System.Windows.Window messagewindow = null;
            switch (config.Notification)
            {
                case NotificationType.Top:
                    messagewindow = new Views.NotificationTopWindow(newtrack, timetostayopen);
                    break;
                case NotificationType.RightBottom:
                    messagewindow = new Views.NotificationRightBottomWindow(newtrack, timetostayopen);
                    break;
            }
            messagewindow.Show();
            lastwindow = messagewindow;
        }
    }

    public enum NotificationType
    {
        None,
        Top,
        RightBottom
    }
}
