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
    /// Interaction logic for TrackInformationWindow.xaml
    /// </summary>
    public partial class TrackInformationWindow : MahApps.Metro.Controls.MetroWindow
    {
        TrackInformationsView content;
        public TrackInformationWindow(Music.Track track)
        {
            content = new TrackInformationsView(track);
            this.Content = content;
            content.CloseRequest += (s, e) => this.Close();
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            content.Dispose();
        }
    }
}
