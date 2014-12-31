using System.Windows;

namespace Hurricane.MagicArrow.DockManager
{
    /// <summary>
    /// Interaction logic for DockRangeWindow.xaml
    /// </summary>
    public partial class DockRangeWindow : Window
    {
        public DockRangeWindow(double left,double height)
        {
            InitializeComponent();
            this.Top = 0;
            this.Height = height;
            this.Left = left;
        }
    }
}
