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
        double WindowLeft;

        public EqualizerWindow(Music.CSCoreEngine cscore, double left, double top)
        {
            InitializeComponent();
            this.cscore = cscore;
            this.WindowLeft = left;
            this.Left = left;
            this.Top = top;
            this.Loaded += EqualizerWindow_Loaded;
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
                    DoubleAnimation doubleanimation = new DoubleAnimation(this.Left, this.Left - 25, TimeSpan.FromMilliseconds(100));
                    Storyboard.SetTargetProperty(doubleanimation, new PropertyPath(Window.LeftProperty));
                    Storyboard.SetTarget(doubleanimation, this);
                    story.Children.Add(doubleanimation);

                    story.Completed += (s, es) => { CanClose = true; this.Close(); };
                    story.Begin(this);
                }
            }
        }

        public void SetLeft(double value)
        {
            this.Left = value;
        }

        void EqualizerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            Storyboard story = new Storyboard();
            DoubleAnimation doubleanimation = new DoubleAnimation(WindowLeft - 25, WindowLeft, TimeSpan.FromMilliseconds(100));
            Storyboard.SetTargetProperty(doubleanimation, new PropertyPath(Window.LeftProperty));
            Storyboard.SetTarget(doubleanimation, this);
            story.Children.Add(doubleanimation);

            story.Begin(this);
             * */
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
