using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Hurricane.Notification.Views
{
    /// <summary>
    /// Interaction logic for NotificationRightBottomWindow.xaml
    /// </summary>
    public partial class NotificationRightBottomWindow : Window
    {
        public NotificationRightBottomWindow(Music.Track track, TimeSpan timestayopened)
        {
            this.CurrentTrack = track;
            InitializeComponent();
            this.Top = System.Windows.SystemParameters.WorkArea.Height - this.Height;
            this.Width = System.Windows.SystemParameters.WorkArea.Width / 4;
            this.Left = System.Windows.SystemParameters.WorkArea.Width - this.Width;
            this.Closing += NotificationRightBottomWindow_Closing;
            this.MouseMove += NotificationRightBottomWindow_MouseMove;
            if (track.Image != null) { imgAlbum.Visibility = System.Windows.Visibility.Visible; imgPlaceholder.Visibility = System.Windows.Visibility.Collapsed; }
            System.Threading.Thread t = new System.Threading.Thread(() => { System.Threading.Thread.Sleep(timestayopened); if (!IsClosing) Application.Current.Dispatcher.Invoke(() => MoveOut()); });
            t.IsBackground = true;
            t.Start();
        }

        public BitmapImage TrackImage
        {
            get
            {
                if (CurrentTrack.Image == null) return null;
                return CurrentTrack.Image;
            }
        }

        private bool IsClosing;
        private bool CanClose = false;
        void NotificationRightBottomWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsClosing) MoveOut();
        }

        void NotificationRightBottomWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
            Storyboard.SetTargetProperty(animation, new PropertyPath(Window.OpacityProperty));
            story.Completed += story_Completed;
            story.Begin(this);
        }

        void story_Completed(object sender, EventArgs e)
        {
            CanClose = true;
            this.Close();
        }

        public Music.Track CurrentTrack { get; set; }
    }
}
