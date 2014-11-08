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
    /// Interaktionslogik für CreateNewPlaylistWindow.xaml
    /// </summary>
    public partial class CreateNewPlaylistWindow : MahApps.Metro.Controls.MetroWindow, System.ComponentModel.INotifyPropertyChanged
    {
        public CreateNewPlaylistWindow()
        {
            InitializeComponent();
        }

        public CreateNewPlaylistWindow(string CurrentName) : this()
        {
            this.PlaylistName = CurrentName;
            this.Title = Application.Current.FindResource("renameplaylist").ToString();
            this.btnOK.Content = Application.Current.FindResource("rename").ToString();
            this.txt.SelectAll();
        }

        private String playlistname;
        public String PlaylistName
        {
            get { return playlistname; }
            set
            {
                if(value != playlistname){
                    playlistname = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("PlaylistName"));
                    }
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
