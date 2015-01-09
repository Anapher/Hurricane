using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;
using Hurricane.Music;
using Hurricane.Utilities;
using Hurricane.Utilities.Native;
using Hurricane.Views.UserControls;
using MahApps.Metro.Controls;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for EqualizerWindow.xaml
    /// </summary>
    public partial class EqualizerWindow : MetroWindow
    {
        public event EventHandler BeginCloseAnimation;

        public EqualizerWindow(RECT rect, double width)
        {
            InitializeComponent();
            this.SetPosition(rect, width);
            this.Closing += EqualizerWindow_Closing;
        }

        bool _isClosing = false;
        bool _canClose = false;
        void EqualizerWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!_canClose)
            {
                e.Cancel = true;
                if (!_isClosing)
                {
                    _isClosing = true;
                    Storyboard story = new Storyboard();
                    DoubleAnimation doubleanimation = new DoubleAnimation(IsLeft ? this.Left - 25 : this.Left + 25, TimeSpan.FromMilliseconds(100));
                    Storyboard.SetTargetProperty(doubleanimation, new PropertyPath(LeftProperty));
                    Storyboard.SetTarget(doubleanimation, this);
                    story.Children.Add(doubleanimation);

                    story.Completed += (s, es) => { _canClose = true; this.Close(); };
                    story.Begin(this);

                    if (BeginCloseAnimation != null) BeginCloseAnimation(this, EventArgs.Empty);
                }
            }
        }

        protected const double width = 300; //the widht of this window. we can't use this.ActualWidth because the window isn't always initialized
        protected bool IsLeft;
        public void SetPosition(RECT parentRecantgle, double windowWidth)
        {
            this.Top = parentRecantgle.top + 25;
            if (parentRecantgle.left + windowWidth + windowWidth - WpfScreen.AllScreensWidth > 0) //If left from the parent isn't 300 space
            {
                this.Left = parentRecantgle.left - windowWidth;
                IsLeft = false;
            }
            else
            {
                this.Left = parentRecantgle.left + windowWidth;
                IsLeft = true;
            }
        }

        private void EqualizerView_OnWantClose(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}