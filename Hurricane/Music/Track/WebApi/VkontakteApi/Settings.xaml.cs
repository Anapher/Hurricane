using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hurricane.Settings;

namespace Hurricane.Music.Track.WebApi.VkontakteApi
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        public VkontakteApi VkontakteApi { get; private set; }

        public Settings(VkontakteApi vkontakteApi)
        {
            VkontakteApi = vkontakteApi;
            InitializeComponent();

            EmailTextBox.Text = vkontakteApi.Credentials.Field1;
            PasswordBox.Password = vkontakteApi.Credentials.Field2;
            CheckIfCanLogIn();
        }

        private async void LoginIn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
                return;

            IsEnabled = false;
            try
            {
                await VkontakteApi.Search("garcon", EmailTextBox.Text, PasswordBox.Password);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Application.Current.Resources["AnErrorOccurred"].ToString(), ex.Message),
                    Application.Current.Resources["Error"].ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                IsEnabled = true;
                return;
            }
            VkontakteApi.Credentials.Field1 = EmailTextBox.Text;
            VkontakteApi.Credentials.Field2 = PasswordBox.Password;

            var passwords = HurricaneSettings.Instance.Config.Passwords;

            if (passwords.Any(x => x.Id == VkontakteApi.Id))
                passwords.Remove(passwords.First(x => x.Id == VkontakteApi.Id));

            passwords.Add(VkontakteApi.Credentials);
            VkontakteApi.OnIsEnabledChanged();
            IsEnabled = true;
            CheckIfCanLogIn();
        }

        private void CheckIfCanLogIn()
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                LogInButton.IsEnabled = false;
                return;
            }

            if (EmailTextBox.Text == VkontakteApi.Credentials.Field1 &&
                PasswordBox.Password == VkontakteApi.Credentials.Field2)
            {
                LogInButton.IsEnabled = false;
                return;
            }
            LogInButton.IsEnabled = true;
        }

        private void EmailTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfCanLogIn();
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            CheckIfCanLogIn();
        }

        private void PasswordBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                LoginIn_Click(sender, e);
        }
    }
}