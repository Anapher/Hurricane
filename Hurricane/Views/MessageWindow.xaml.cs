using System.Windows;
using MahApps.Metro.Controls;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : MetroWindow
    {
        public MessageWindow(string message, string title, bool cancancel)
        {
            InitializeComponent();
            if (!cancancel) btnCancel.Visibility = Visibility.Collapsed;
            this.Title = title;
            this.txt.Text = message;
        }
    }
}
