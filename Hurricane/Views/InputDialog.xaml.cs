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
    /// Interaction logic for CreateNewPlaylistWindow.xaml
    /// </summary>
    public partial class InputDialog : MahApps.Metro.Controls.MetroWindow, System.ComponentModel.INotifyPropertyChanged
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

        private String resulttext;
        public String ResultText
        {
            get { return resulttext; }
            set
            {
                if (value != resulttext)
                {
                    resulttext = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("ResultText"));
                    }
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
