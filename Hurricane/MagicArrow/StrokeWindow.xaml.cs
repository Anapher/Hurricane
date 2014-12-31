using System.Windows;
using System.Windows.Media;

namespace Hurricane.MagicArrow
{
    /// <summary>
    /// Interaction logic for StrokeWindow.xaml
    /// </summary>
    public partial class StrokeWindow : Window
    {
        public StrokeWindow(double height, double left, Side side)
        {
            InitializeComponent();
            this.StrokeWidth = 3;
            this.Top = 0;
            this.SetLeft(left, side);
            this.Height = height;
            this.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            this.CurrentSide = side;
        }

        public Side CurrentSide { get; set; }
        public double StrokeWidth { get; set; }

        public void SetLeft(double left, Side side)
        {
            if (side == Side.Left) { this.Left = left - (20 - StrokeWidth); } else { this.Left = left - StrokeWidth; }
        }

        private double _currentleft;
        public void MoveInvisible()
        {
            this._currentleft = this.Left;
            this.Left = -100;
            this.IsInvisible = true;
        }

        public void MoveVisible()
        {
            this.Left = this._currentleft;
            this.IsInvisible = false;
        }

        public bool IsInvisible { get; set; }

        public static bool PositionIsOk(Side side, double position, double width, double screenwidth)
        {
            return side == Side.Left ? position >= width : position <= screenwidth - width - 1;
        }
    }
}