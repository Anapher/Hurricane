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
    /// Interaktionslogik für MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : MahApps.Metro.Controls.MetroWindow
    {
        public MessageWindow(string message, string title, bool cancancel, bool loadstringsfromresources =false)
        {
            InitializeComponent();
            if (!cancancel) btnCancel.Visibility = System.Windows.Visibility.Collapsed;
            this.Title = loadstringsfromresources ? Application.Current.FindResource(title).ToString() : title;
            this.txt.Text = loadstringsfromresources ? Application.Current.FindResource(message).ToString() : message;
        }
    }
}
