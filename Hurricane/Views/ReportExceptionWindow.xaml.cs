using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Exceptionless;
using Hurricane.Settings;
using MahApps.Metro.Controls;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for ReportExceptionWindow.xaml
    /// </summary>
    public partial class ReportExceptionWindow : MetroWindow, INotifyPropertyChanged
    {
        public ReportExceptionWindow(Exception error)
        {
            InitializeComponent();
            this.Error = error;
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
            ex.SetUserDescription(null, txtNote.Text);
            ex.Submit();
            ((Button)sender).IsEnabled = false;
            prg.IsIndeterminate = true;
            await Task.Run(() => ExceptionlessClient.Default.ProcessQueue());
            prg.IsIndeterminate = false;
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
