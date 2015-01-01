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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hurricane.ViewModels;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public static readonly DependencyProperty ShowAboutProperty = DependencyProperty.Register("ShowAbout", typeof(bool), typeof(SettingsView), new PropertyMetadata(true, ShowAboutPropertyChangedCallback));

        private static void ShowAboutPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((SettingsView) dependencyObject).AboutListBoxItem.Visibility = (bool) dependencyPropertyChangedEventArgs.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public bool ShowAbout
        {
            get { return (bool)GetValue(ShowAboutProperty); }
            set { SetValue(ShowAboutProperty, value); }
        }

        public SettingsView()
        {
            InitializeComponent();
        }

        private void SettingChanged(object sender, RoutedEventArgs e)
        {
            SettingsViewModel.Instance.StateChanged();
        }
    }
}
