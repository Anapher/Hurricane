using System.ComponentModel;
using Hurricane.Music;
using MahApps.Metro.Controls;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for TrackInformationWindow.xaml
    /// </summary>
    public partial class TrackInformationWindow : MetroWindow
    {
        readonly TrackInformationView content;
        public TrackInformationWindow(Track track)
        {
            content = new TrackInformationView(track);
            this.Content = content;
            content.CloseRequest += (s, e) => this.Close();
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            content.Dispose();
        }
    }
}
