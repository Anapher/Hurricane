using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using Exceptionless;
using Newtonsoft.Json;
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
            ex.SetUserDescription(string.Empty, NoteTextBox.Text);
            ex.AddObject(HurricaneSettings.Instance.Config, "HurricaneSettings", null, null, true);

            if (HurricaneSettings.Instance.IsLoaded)
            {
                using (var sw = new StringWriter())
                {
                    XmlAttributeOverrides overrides = new XmlAttributeOverrides(); //DONT serialize the passwords and send them to me!
                    XmlAttributes attribs = new XmlAttributes {XmlIgnore = true};
                    attribs.XmlElements.Add(new XmlElementAttribute("Passwords"));
                    overrides.Add(typeof(ConfigSettings), "Passwords", attribs);

                    var xmls = new XmlSerializer(typeof(ConfigSettings), overrides);
                    xmls.Serialize(sw, HurricaneSettings.Instance.Config);

                    var doc = new XmlDocument();
                    doc.LoadXml(sw.ToString());
                    ex.SetProperty("HurricaneSettings", JsonConvert.SerializeXmlNode(doc));
                }
            }

            ex.Submit();
            ((Button)sender).IsEnabled = false;
            StatusProgressBar.IsIndeterminate = true;
            await ExceptionlessClient.Default.ProcessQueueAsync();
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
