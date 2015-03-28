using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Hurricane.Music.Track;

namespace Hurricane.Notification.Views
{
    /// <summary>
    /// Interaction logic for NotificationRightBottomWindow.xaml
    /// </summary>
    public partial class NotificationRightBottomWindow
    {
        public NotificationRightBottomWindow(PlayableBase track, TimeSpan timestayopened)
        {
            CurrentTrack = track;
            InitializeComponent();
            Top = SystemParameters.WorkArea.Height - Height;
            Width = SystemParameters.WorkArea.Width / 4;
            Left = SystemParameters.WorkArea.Width - Width;
            Closing += NotificationRightBottomWindow_Closing;
            MouseMove += NotificationRightBottomWindow_MouseMove;
            Thread t = new Thread(() =>
            {
                Thread.Sleep(timestayopened);
                if (!_isClosing) Application.Current.Dispatcher.Invoke(MoveOut);
            }) { IsBackground = true };
            t.Start();
        }

        private bool _isClosing;
        private bool _canClose;
        void NotificationRightBottomWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isClosing) MoveOut();
        }

        void NotificationRightBottomWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!_isClosing) MoveOut();
            e.Cancel = !_canClose;
        }

        void MoveOut()
        {
            _isClosing = true;
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
            _canClose = true;
            Close();
        }

        public PlayableBase CurrentTrack { get; set; }
    }
}
