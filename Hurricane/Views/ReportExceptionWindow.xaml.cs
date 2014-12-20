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
using System.ComponentModel;
using Exceptionless;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaktionslogik für ReportExceptionWindow.xaml
    /// </summary>
    public partial class ReportExceptionWindow : MahApps.Metro.Controls.MetroWindow, INotifyPropertyChanged
    {
        public ReportExceptionWindow(Exception error)
        {
            InitializeComponent();
            this.Error = error;
            if (Hurricane.Settings.HurricaneSettings.Instance.Loaded) Hurricane.Settings.HurricaneSettings.Instance.Save();
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
            ex.SetUserDescription(txtNote.Text);
            ex.Submit();
            ((Button)sender).IsEnabled = false;
            prg.IsIndeterminate = true;
            await Task.Run(() => ExceptionlessClient.Current.ProcessQueue());
            prg.IsIndeterminate = false;
            App.Current.Shutdown();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        #region INotifyPropertyChanged
        protected void OnPropertyChanged(string propertyname)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
