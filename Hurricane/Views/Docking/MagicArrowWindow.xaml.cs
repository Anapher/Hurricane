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
        public event MouseButtonEventHandler Click;

        public double InvisibleLeft { get; set; }
        public double VisibleLeft { get; set; }

        private bool _closeAllowed;

        /// <summary>
        /// Shows the magic arrow
        /// </summary>
        /// <param name="top">The top position</param>
        /// <param name="invisibleLeft">The x position where the <b>Magic Arrow</b> wouldn't be visible (for the animation)</param>
        /// <param name="visibleLeft">The correct left position of the <b>Magic Arrow</b></param>
        /// <param name="side">The side of the <b>Magic Arrow</b></param>
        public MagicArrowWindow(double top, double invisibleLeft, double visibleLeft, Side side)
        {
            InvisibleLeft = invisibleLeft;
            VisibleLeft = visibleLeft;

            InitializeComponent();

            Top = top - Height  / 2;
            Left = visibleLeft;
            if (side == Side.Right)
            {
                //We rotate the Magic Arrow
                MagicArrow.RenderTransformOrigin = new Point(.5, .5);
                MagicArrow.RenderTransform = new ScaleTransform(-1, 1);
            }
        }

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

            var outAnimation = new DoubleAnimation(InvisibleLeft, TimeSpan.FromMilliseconds(400))
            {
                EasingFunction = new CircleEase {EasingMode = EasingMode.EaseOut}
            };

            outAnimation.Completed += (s, args) => Close();
            BeginAnimation(LeftProperty, outAnimation);
            e.Cancel = true;
            _closeAllowed = true;
        }
    }
}
