using System;
using System.IO;
using System.Windows;
using Hurricane.Settings.Themes;
using MahApps.Metro.Controls;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for ThemeCreatorWindow.xaml
    /// </summary>
    public partial class ThemeCreatorWindow : MetroWindow
    {
        public ThemeCreatorWindow(ThemeSource theme)
        {
            this.DataContext = theme;
            InitializeComponent();

            this.Theme = theme;
            txtName.IsEnabled = false;
        }

        public ThemeCreatorWindow()
        {
            var theme = new ThemeSource();
            this.DataContext = theme;
            InitializeComponent();

            this.Theme = theme;
        }

        public ThemeSource Theme { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo("Themes");
                if (!di.Exists) di.Create();
                Theme.Save();
                this.DialogResult = true;
            }
            catch (Exception)
            {
                MessageWindow window = new MessageWindow(Application.Current.Resources["SaveError"].ToString(), Application.Current.Resources["Error"].ToString(), false) { Owner = this };
                window.ShowDialog();
            }
        }
    }
}
