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

namespace Hurricane.MagicArrow
{
    /// <summary>
    /// Interaktionslogik für MagicArrowWindow.xaml
    /// </summary>
    public partial class MagicArrowWindow : Window
    {
        public event EventHandler MoveVisible;
        private bool CanClose = true;

        public MagicArrowWindow(int top)
        {
            InitializeComponent();
            this.Top = top - this.Height / 2;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MoveVisible != null)
                MoveVisible(this, EventArgs.Empty);
            this.Close();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            if (this.Left < -10)
                this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (CanClose)
            {
                DoubleAnimation animation = new DoubleAnimation(-12, TimeSpan.FromMilliseconds(400));
                this.BeginAnimation(Window.LeftProperty, animation);
                e.Cancel = true;
                CanClose = false;
            }
        }
    }
}
