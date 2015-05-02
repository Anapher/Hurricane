using System.Windows;
using Hurricane.MagicArrow;

namespace Hurricane.Views.Docking
{
    /// <summary>
    /// Interaction logic for MagicTriggerWindow.xaml
    /// </summary>
    public partial class MagicTriggerWindow
    {
        private bool _isMagicTriggerVisible = true;

        public MagicTriggerWindow(double height, double left, double top, Side side)
        {
            InitializeComponent();
            FixedWidth = 3;
            Top = top;
            SetLeft(left, side);
            Height = height;
            CurrentSide = side;
        }

        public double FixedWidth { get; set; }
        public Side CurrentSide { get; set; }

        public bool IsMagicTriggerVisible
        {
            get { return _isMagicTriggerVisible; }
            set
            {
                Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                _isMagicTriggerVisible = value;
            }
        }

        public void SetLeft(double left, Side side)
        {
            if (side == Side.Left) { Left = left - (20 - FixedWidth); } else { Left = left - FixedWidth; }
        }
    }
}