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
