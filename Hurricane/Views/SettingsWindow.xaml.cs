using System.Windows;
using Hurricane.ViewModels;
using MahApps.Metro.Controls;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            this.Loaded += SettingsWindow_Loaded;
        }

        void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SettingsViewModel.Instance.Load();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = TabControl.SelectedIndex == 0 ? 1 : 0;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
