using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Exceptionless;
using Hurricane.Settings;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for ReportExceptionWindow.xaml
    /// </summary>
    public partial class ReportExceptionWindow : INotifyPropertyChanged
    {
        public ReportExceptionWindow(Exception error)
        {
            InitializeComponent();
            Error = error;
            if (HurricaneSettings.Instance.IsLoaded) HurricaneSettings.Instance.Save();
        }

        private Exception _error;
        public Exception Error
        {
            get { return _error; }
            set { _error = value; OnPropertyChanged("Error"); }
        }

        private async void ButtonSendErrorReport_Click(object sender, RoutedEventArgs e)
        {
            var ex = Error.ToExceptionless();
            ex.SetUserDescription(null, NoteTextBox.Text);
            ex.Submit();
            ((Button)sender).IsEnabled = false;
            StatusProgressBar.IsIndeterminate = true;
            await Task.Run(() => ExceptionlessClient.Default.ProcessQueue());
            StatusProgressBar.IsIndeterminate = false;
            Application.Current.Shutdown();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #region INotifyPropertyChanged
        protected void OnPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
