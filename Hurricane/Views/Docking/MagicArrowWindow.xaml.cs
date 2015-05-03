using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Hurricane.MagicArrow;

namespace Hurricane.Views.Docking
{
    /// <summary>
    /// Interaction logic for MagicArrowWindow.xaml
    /// </summary>
    public partial class MagicArrowWindow
    {
        private bool _closeAllowed;

        /// <summary>
        /// Shows the magic arrow
        /// </summary>
        /// <param name="top">The top position</param>
        /// <param name="left">The left position of the <b>Magic Arrow</b></param>
        /// <param name="side">The side of the <b>Magic Arrow</b></param>
        public MagicArrowWindow(double top, double left, Side side)
        {
            InitializeComponent();

            Top = top - Height / 2;
            Left = left;
            if (side == Side.Right)
            {
                //We rotate the Magic Arrow
                MagicArrow.RenderTransformOrigin = new Point(.5, .5);
                MagicArrow.RenderTransform = new ScaleTransform(-1, 1);
            }
        }

        public event MouseButtonEventHandler Click;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (Click != null)
                Click(this, e);

            _closeAllowed = true; //We skip the animation
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_closeAllowed) return;

            var outStoryboard = (Storyboard)Resources["FadeOutStoryboard"];
            outStoryboard.Completed += (sender, args) => Close();
            outStoryboard.Begin(this);

            e.Cancel = true;
            _closeAllowed = true;
        }
    }
}
