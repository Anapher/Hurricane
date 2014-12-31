using System;
using System.ComponentModel;
using MahApps.Metro.Controls;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for CreateNewPlaylistWindow.xaml
    /// </summary>
    public partial class InputDialog : MetroWindow, INotifyPropertyChanged
    {
        public InputDialog()
        {
            InitializeComponent();
        }

        public InputDialog(string title, string message, string buttonoktext, string defaulttext)
            : this()
        {
            this.ResultText = defaulttext;
            this.Title = title;
            this.txtMessage.Text = message;
            this.btnOK.Content = buttonoktext;
            this.txt.SelectAll();
        }

        private String _resulttext;
        public String ResultText
        {
            get { return _resulttext; }
            set
            {
                if (value != _resulttext)
                {
                    _resulttext = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("ResultText"));
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
