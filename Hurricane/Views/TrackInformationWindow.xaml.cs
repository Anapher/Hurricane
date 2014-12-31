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
        readonly TrackInformationsView content;
        public TrackInformationWindow(Track track)
        {
            content = new TrackInformationsView(track);
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
