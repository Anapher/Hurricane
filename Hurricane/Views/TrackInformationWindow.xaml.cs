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
    /// Interaktionslogik für TrackInformationWindow.xaml
    /// </summary>
    public partial class TrackInformationWindow : MahApps.Metro.Controls.MetroWindow
    {
        public TrackInformationWindow(Music.Track track)
        {
            this.CurrentTrack = track;
            InitializeComponent();
        }

        public Music.Track CurrentTrack { get; set; }
    }
}
