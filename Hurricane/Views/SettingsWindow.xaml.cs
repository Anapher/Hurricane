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

namespace Hurricane.Views
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MahApps.Metro.Controls.MetroWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            this.Loaded += SettingsWindow_Loaded;
        }

        void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModels.SettingsViewModel.Instance.Load(this);
        }

        void SomethingChanged(object sender, RoutedEventArgs e)
        {
            ViewModels.SettingsViewModel.Instance.StateChanged();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Views.AboutWindow window = new AboutWindow();
            window.Owner = this;
            window.ShowDialog();
        }
    }
}
