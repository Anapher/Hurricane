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
using Hurricane.Settings;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaktionslogik für UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : MahApps.Metro.Controls.MetroWindow
    {
        public UpdateService UpdateService { get; set; }

        public UpdateWindow(UpdateService updateservice)
        {
            this.UpdateService = updateservice;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            gridState.Visibility = System.Windows.Visibility.Visible;
            UpdateService.Update();
        }

        private void btncancel_Click(object sender, RoutedEventArgs e)
        {
            UpdateService.CancelUpdate();
            this.Close();
        }
    }
}
