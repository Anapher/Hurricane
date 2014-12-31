using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Hurricane.Music;

namespace Hurricane.Notification.Views
{
    /// <summary>
    /// Interaction logic for NotificationRightBottomWindow.xaml
    /// </summary>
    public partial class NotificationRightBottomWindow : Window
    {
        public NotificationRightBottomWindow(Track track, TimeSpan timestayopened)
        {
            this.CurrentTrack = track;
            InitializeComponent();
            this.Top = SystemParameters.WorkArea.Height - this.Height;
            this.Width = SystemParameters.WorkArea.Width / 4;
            this.Left = SystemParameters.WorkArea.Width - this.Width;
            this.Closing += NotificationRightBottomWindow_Closing;
            this.MouseMove += NotificationRightBottomWindow_MouseMove;
            Thread t = new Thread(() =>
            {
                Thread.Sleep(timestayopened);
                if (!IsClosing) Application.Current.Dispatcher.Invoke(MoveOut);
            }) { IsBackground = true };
            t.Start();
        }

        private bool IsClosing;
        private bool CanClose = false;
        void NotificationRightBottomWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsClosing) MoveOut();
        }

        void NotificationRightBottomWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!IsClosing) MoveOut();
            e.Cancel = !CanClose;
        }

        void MoveOut()
        {
            IsClosing = true;
            DoubleAnimation animation = new DoubleAnimation(0.85, 0, TimeSpan.FromMilliseconds(300));

            Storyboard story = new Storyboard();
            story.Children.Add(animation);
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            story.Completed += story_Completed;
            story.Begin(this);
        }

        void story_Completed(object sender, EventArgs e)
        {
            CanClose = true;
            this.Close();
        }

        public Track CurrentTrack { get; set; }
    }
}
