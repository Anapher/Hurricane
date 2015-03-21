using System.Windows;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow
    {
        public MessageWindow(string message, string title, bool cancancel, string affirmativeButtonText = null, string negativeButtonText = null)
        {
            InitializeComponent();
            if (!cancancel) NegativeButton.Visibility = Visibility.Collapsed;
            Title = title;
            MessageTextBlock.Text = message;
            if (!string.IsNullOrEmpty(affirmativeButtonText))
                PositiveButton.Content = affirmativeButtonText;
            if (!string.IsNullOrEmpty(negativeButtonText))
                NegativeButton.Content = negativeButtonText;
        }
    }
}
