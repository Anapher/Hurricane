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

namespace Hurricane.MagicArrow
{
    /// <summary>
    /// Interaktionslogik für StrokeWindow.xaml
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

        private double currentleft;
        public void MoveInvisible()
        {
            this.currentleft = this.Left;
            this.Left = -100;
            this.IsInvisible = true;
        }

        public void MoveVisible()
        {
            this.Left = this.currentleft;
            this.IsInvisible = false;
        }

        public bool IsInvisible { get; set; }

        public static bool PositionIsOk(Side side, double position, double width, double screenwidth)
        {
            if (side == Side.Left)
            {
                return position >= width;
            }
            else
            {
                return position <= screenwidth - width - 1;
            }
        }
    }
}