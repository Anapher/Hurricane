using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Notification
{
    class NotificationService
    {
        public NotificationService(Music.CSCoreEngine cscore)
        {
            cscore.TrackChanged += cscore_TrackChanged;
        }

        void cscore_TrackChanged(object sender, Music.TrackChangedEventArgs e)
        {
            TrackChanged(e.NewTrack);
        }

        private System.Windows.Window lastwindow;
        protected Music.Track lasttrack;

        void TrackChanged(Music.Track newtrack)
        {
            Settings.ConfigSettings config = Settings.HurricaneSettings.Instance.Config;
            if (config.DisableNotificationInGame && Utilities.WindowHelper.WindowIsFullscreen(Utilities.Native.UnsafeNativeMethods.GetForegroundWindow())) return;
            ShowNotification(newtrack, config.Notification);

            lasttrack = newtrack;
        }

        protected void ShowNotification(Music.Track track, NotificationType type)
        {
            Settings.ConfigSettings config = Settings.HurricaneSettings.Instance.Config;
            if (config.Notification == NotificationType.None) return;
            if (lastwindow != null && lastwindow.Visibility == System.Windows.Visibility.Visible) lastwindow.Close();
            TimeSpan timetostayopen = TimeSpan.FromMilliseconds(config.NotificationShowTime);

            System.Windows.Window messagewindow = null;
            switch (config.Notification)
            {
                case NotificationType.Top:
                    messagewindow = new Views.NotificationTopWindow(track, timetostayopen);
                    break;
                case NotificationType.RightBottom:
                    messagewindow = new Views.NotificationRightBottomWindow(track, timetostayopen);
                    break;
            }
            messagewindow.Show();
            lastwindow = messagewindow;
        }

        public void Test(NotificationType type)
        {
            Music.Track track2use = lasttrack;
            if (track2use == null)
            {
                track2use = new Music.Track();
                track2use.Artist = "Alkalinee";
                track2use.Title = "Sample Track";
                track2use.Duration = "03:26";
                track2use.kHz = 44;
                track2use.Path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                track2use.Extension = "MP3";
            }
            ShowNotification(track2use, type);
        }
    }

    public enum NotificationType
    {
        None,
        Top,
        RightBottom
    }
}
