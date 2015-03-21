using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;
using Hurricane.Utilities;
using Hurricane.Utilities.Native;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for EqualizerWindow.xaml
    /// </summary>
    public partial class EqualizerWindow
    {
        public event EventHandler BeginCloseAnimation;

        public EqualizerWindow(RECT rect, double width)
        {
            InitializeComponent();
            SetPosition(rect, width);
            Closing += EqualizerWindow_Closing;
        }

        bool _isClosing;
        bool _canClose;
        void EqualizerWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!_canClose)
            {
                e.Cancel = true;
                if (!_isClosing)
                {
                    _isClosing = true;
                    Storyboard story = new Storyboard();
                    DoubleAnimation doubleanimation = new DoubleAnimation(_isLeft ? Left - 25 : Left + 25, TimeSpan.FromMilliseconds(100));
                    Storyboard.SetTargetProperty(doubleanimation, new PropertyPath(LeftProperty));
                    Storyboard.SetTarget(doubleanimation, this);
                    story.Children.Add(doubleanimation);

                    story.Completed += (s, es) => { _canClose = true; Close(); };
                    story.Begin(this);

                    if (BeginCloseAnimation != null) BeginCloseAnimation(this, EventArgs.Empty);
                }
            }
        }

        private bool _isLeft;
        public void SetPosition(RECT parentRecantgle, double windowWidth)
        {
            Top = parentRecantgle.top + 25;
            if (parentRecantgle.left + windowWidth + windowWidth - WpfScreen.MostRightX > 0) //If left from the parent isn't 300 space
            {
                Left = parentRecantgle.left - windowWidth;
                _isLeft = false;
            }
            else
            {
                Left = parentRecantgle.left + windowWidth;
                _isLeft = true;
            }
        }

        private void EqualizerView_OnWantClose(object sender, EventArgs e)
        {
            Close();
        }
    }
}