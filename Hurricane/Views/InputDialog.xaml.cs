using System.ComponentModel;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for CreateNewPlaylistWindow.xaml
    /// </summary>
    public partial class InputDialog : INotifyPropertyChanged
    {
        public InputDialog()
        {
            InitializeComponent();
        }

        public InputDialog(string title, string message, string buttonoktext, string defaulttext)
            : this()
        {
            ResultText = defaulttext;
            Title = title;
            MessageTextBlock.Text = message;
            OkButton.Content = buttonoktext;
            ResultTextBox.SelectAll();
        }

        private string _resulttext;
        public string ResultText
        {
            get { return _resulttext; }
            set
            {
                if (value != _resulttext)
                {
                    _resulttext = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ResultText"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
