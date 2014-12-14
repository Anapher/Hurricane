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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for EqualizerWindow.xaml
    /// </summary>
    public partial class EqualizerWindow : MahApps.Metro.Controls.MetroWindow
    {
        Music.CSCoreEngine cscore;
        public event EventHandler BeginCloseAnimation;

        public EqualizerWindow(Music.CSCoreEngine cscore, Utilities.Native.RECT rect,double width)
        {
            InitializeComponent();
            this.cscore = cscore;
            this.SetPosition(rect, width);
            this.Closing += EqualizerWindow_Closing;
        }

        bool IsClosing = false;
        bool CanClose = false;
        void EqualizerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CanClose)
            {
                e.Cancel = true;
                if (!IsClosing)
                {
                    IsClosing = true;
                    Storyboard story = new Storyboard();
                    DoubleAnimation doubleanimation = new DoubleAnimation(IsLeft ? this.Left - 25 : this.Left + 25, TimeSpan.FromMilliseconds(100));
                    Storyboard.SetTargetProperty(doubleanimation, new PropertyPath(Window.LeftProperty));
                    Storyboard.SetTarget(doubleanimation, this);
                    story.Children.Add(doubleanimation);

                    story.Completed += (s, es) => { CanClose = true; this.Close(); };
                    story.Begin(this);

                    if (BeginCloseAnimation != null) BeginCloseAnimation(this, EventArgs.Empty);
                }
            }
        }

        protected const double width = 300; //the wídht of this window. we can't use this.ActualWidth because the window isn't always initialized
        protected bool IsLeft;
        public void SetPosition(Utilities.Native.RECT ParentRecantgle, double width)
        {
            this.Top = ParentRecantgle.top + 25;
            if (ParentRecantgle.left + width + width - Utilities.WpfScreen.AllScreensWidth > 0) //If left from the parent isn't 300 space
            {
                this.Left = ParentRecantgle.left - width;
                IsLeft = false;
            }
            else
            {
                this.Left = ParentRecantgle.left + width;
                IsLeft = true;
            }
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
