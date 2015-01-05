using System.Windows;

namespace Hurricane.MagicArrow.DockManager
{
    /// <summary>
    /// Interaction logic for DockRangeWindow.xaml
    /// </summary>
    public partial class DockRangeWindow : Window
    {
        public DockRangeWindow(double left, double height) : this(0, left, height, 300) { }

        public DockRangeWindow(double top, double left, double height, double width)
        {
            InitializeComponent();
            this.Top = top;
            this.Left = left;
            this.Height = height;
            this.Width = width;
        }
    }
}
