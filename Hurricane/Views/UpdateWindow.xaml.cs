using System.Windows;
using System.Windows.Controls;
using Hurricane.Settings;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow
    {
        public UpdateService UpdateService { get; set; }

        public UpdateWindow(UpdateService updateservice)
        {
            UpdateService = updateservice;
            InitializeComponent();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            StateGrid.Visibility = Visibility.Visible;
            UpdateService.Update();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateService.CancelUpdate();
            Close();
        }
    }
}
