using System.Windows.Media;
using Hurricane.Utilities;

namespace Hurricane.MagicArrow
{
    /// <summary>
    /// Interaction logic for StrokeWindow.xaml
    /// </summary>
    public partial class StrokeWindow
    {

        private double _currentleft;

        public StrokeWindow(double height, double left, double top, Side side)
        {
            InitializeComponent();
            StrokeWidth = 3;
            Top = top;
            SetLeft(left, side);
            Height = height;
            SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            CurrentSide = side;
            if (ShowLines)
            {
                Opacity = 1;
                Background = Brushes.Red;
            }

            SourceInitialized += StrokeWindow_SourceInitialized;
        }

        public Side CurrentSide { get; set; }
        public double StrokeWidth { get; set; }
        public bool IsInvisible { get; set; }
        public static bool ShowLines { get; set; }

        public void SetLeft(double left, Side side)
        {
            if (side == Side.Left) { Left = left - (20 - StrokeWidth); } else { Left = left - StrokeWidth; }
        }

        public void MoveInvisible()
        {
            _currentleft = Left;
            Left = -100;
            IsInvisible = true;
        }

        public void MoveVisible()
        {
            Left = _currentleft;
            IsInvisible = false;
        }

        private void StrokeWindow_SourceInitialized(object sender, System.EventArgs e)
        {
            WindowHelper.HideFromAltTab(this);
        }

        public static bool PositionIsOk(Side side, double position, double width, double screenwidth)
        {
            return side == Side.Left ? position >= width : position <= screenwidth - width - 1;
        }
    }
}