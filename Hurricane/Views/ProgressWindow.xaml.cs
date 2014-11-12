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
    /// Interaktionslogik für ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : MahApps.Metro.Controls.MetroWindow
    {
        public ProgressWindow(string title, bool Indeterminate)
        {
            InitializeComponent();
            this.Title = title;
            this.prg.IsIndeterminate = Indeterminate;
        }

        public void SetText(string text)
        {
           this.Dispatcher.Invoke(() => this.txtinfo.Text = text );
        }

        public void SetProgress(double progress)
        {
            this.Dispatcher.Invoke(() => prg.Value = progress);
        }

        public void SetTitle(string title)
        {
            this.Dispatcher.Invoke(() => this.Title = title);
        }
    }
}
