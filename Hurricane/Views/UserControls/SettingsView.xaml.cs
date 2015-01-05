using System.Windows;
using System.Windows.Controls;
using Hurricane.ViewModels;

namespace Hurricane.Views.UserControls
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public static readonly DependencyProperty ShowAboutProperty = DependencyProperty.Register("ShowAbout", typeof(bool), typeof(SettingsView), new PropertyMetadata(true, ShowAboutPropertyChangedCallback));

        private static void ShowAboutPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var view = ((SettingsView)dependencyObject);
            var newvalue = (bool) dependencyPropertyChangedEventArgs.NewValue;
            view.AboutListBoxItem.Visibility = newvalue ? Visibility.Visible : Visibility.Collapsed;
            if (!newvalue && view.TabControl.SelectedIndex == view.TabControl.Items.Count - 1)
            {
                view.TabControl.SelectedIndex = 0;
            }
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
