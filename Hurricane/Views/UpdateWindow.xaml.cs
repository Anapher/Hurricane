using System.Windows;
using System.Windows.Controls;
using Hurricane.Settings;
using MahApps.Metro.Controls;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : MetroWindow
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
            gridState.Visibility = Visibility.Visible;
            UpdateService.Update();
        }

        private void btncancel_Click(object sender, RoutedEventArgs e)
        {
            UpdateService.CancelUpdate();
            this.Close();
        }
    }
}
