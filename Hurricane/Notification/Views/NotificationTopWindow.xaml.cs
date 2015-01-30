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
    /// Interaction logic for NotificationTopWindow.xaml
    /// </summary>
    public partial class NotificationTopWindow
    {
        public NotificationTopWindow(PlayableBase track, TimeSpan timestayopened)
        {
            this.CurrentTrack = track;
            InitializeComponent();
            this.Width = SystemParameters.WorkArea.Width;
            this.MouseMove += NotificationTopWindow_MouseMove;
            this.Closing += NotificationTopWindow_Closing;
            this.Top = SystemParameters.WorkArea.Top;
            Thread t = new Thread(() =>
            {
                Thread.Sleep(timestayopened);
                if (!_isClosing) Application.Current.Dispatcher.Invoke(MoveOut);
            }) { IsBackground = true };
            t.Start();
        }

        void NotificationTopWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!_isClosing) MoveOut();
            e.Cancel = !_canClose;
        }

        private bool _isClosing;
        private bool _canClose;
        void NotificationTopWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isClosing) MoveOut();
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
            this.Close();
        }

        public PlayableBase CurrentTrack { get; set; }
    }
}
