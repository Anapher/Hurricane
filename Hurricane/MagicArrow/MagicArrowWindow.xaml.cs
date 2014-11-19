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
    /// Interaction logic for MagicArrowWindow.xaml
    /// </summary>
    public partial class MagicArrowWindow : Window
    {
        public event EventHandler MoveVisible;
        public event DragEventHandler FilesDropped;

        private bool CanClose = true;
        private Side currentside;
        public MagicArrowWindow(int top, double fromleft, double toleft, Side side)
        {
            this.FromLeft = fromleft;
            this.ToLeft = toleft;
            InitializeComponent();
            this.Top = top - this.Height / 2;
            this.Left = toleft;
            if (side == Side.Right)
            {
                img.RenderTransformOrigin = new Point(0.5, 0.5);
                ScaleTransform flipTrans = new ScaleTransform();
                flipTrans.ScaleX = -1;
                img.RenderTransform = flipTrans;
            }
            currentside = side;
        }

        public double FromLeft { get; set; }
        public double ToLeft { get; set; }

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
                DoubleAnimation animation = new DoubleAnimation(FromLeft, TimeSpan.FromMilliseconds(400));
                this.BeginAnimation(Window.LeftProperty, animation);
                e.Cancel = true;
                CanClose = false;
            }
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Move;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (FilesDropped != null) FilesDropped(this, e);
            }
        }
    }
}
