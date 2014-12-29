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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TagLib;
using System.ComponentModel;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaktionslogik für TrackInformationsView.xaml
    /// </summary>
    public partial class TrackInformationsView : UserControl, INotifyPropertyChanged, IDisposable
    {
        public event EventHandler CloseRequest;

        public TrackInformationsView(Music.Track track)
        {
            this.TagFile = File.Create(track.Path);
            this.CurrentTrack = track;

            List<Genre> genres = new List<Genre>();
            foreach (var item in TagLib.Genres.Audio)
                genres.Add(new Genre(item, TagFile.Tag.Genres.Contains(item)));
            InitializeComponent();
            lstGenre.ItemsSource = genres;
        }

        public Music.Track CurrentTrack { get; set; }
        public File TagFile { get; set; }

        #region Lyrics
        private void MenuItemOpenLyrics_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaOpenFileDialog ofd = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            ofd.Filter = string.Format("{0} (*.txt)|*.txt|{1} (*.*)|*.*", Application.Current.FindResource("textfile"), Application.Current.FindResource("allfiles"));
            if (ofd.ShowDialog() == true)
            {
                TagFile.Tag.Lyrics = System.IO.File.ReadAllText(ofd.FileName);
                OnPropertyChanged("TagFile.Tag.Lyrics");
            }
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaSaveFileDialog sfd = new Ookii.Dialogs.Wpf.VistaSaveFileDialog();
            sfd.Filter = string.Format("{0} (*.txt)|*.txt|{1} (*.*)|*.*", Application.Current.FindResource("textfile"), Application.Current.FindResource("allfiles"));
            if (sfd.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(sfd.FileName, TagFile.Tag.Lyrics);
            }
        }

        private void MenuItemRemoveAllText_Click(object sender, RoutedEventArgs e)
        {
            txtLyrics.Clear();
        }
        #endregion

        #region INotifyPropertyChanged
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            List<string> genres = new List<string>();
            foreach (var item in (List<Genre>)lstGenre.ItemsSource)
            {
                if (item.IsChecked) genres.Add(item.Text);
            }

            TagFile.Tag.Genres = genres.ToArray();
            TagFile.Save();
            await CurrentTrack.LoadInformations();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            if (CloseRequest != null) CloseRequest(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            TagFile.Dispose();
        }
    }

    public class Genre
    {
        public string Text { get; set; }
        public bool IsChecked { get; set; }

        public Genre(string genre, bool ischecked)
        {
            this.Text = genre;
            this.IsChecked = ischecked;
        }
    }
}
