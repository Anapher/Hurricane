using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hurricane.Music;
using Ookii.Dialogs.Wpf;
using TagLib;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for TrackInformationView.xaml
    /// </summary>
    public partial class TrackInformationView : UserControl, INotifyPropertyChanged, IDisposable
    {
        public event EventHandler CloseRequest;

        public TrackInformationView(Track track)
        {
            this.TagFile = File.Create(track.Path);
            this.CurrentTrack = track;

            List<Genre> genres = Genres.Audio.Select(item => new Genre(item, TagFile.Tag.Genres.Contains(item))).ToList();
            InitializeComponent();
            lstGenre.ItemsSource = genres;
        }

        public Track CurrentTrack { get; set; }
        public File TagFile { get; set; }

        #region Lyrics
        private void MenuItemOpenLyrics_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog ofd = new VistaOpenFileDialog
            {
                Filter = string.Format("{0} (*.txt)|*.txt|{1} (*.*)|*.*", Application.Current.FindResource("TextFiles"), Application.Current.FindResource("AllFiles"))
            };
            if (ofd.ShowDialog() == true)
            {
                TagFile.Tag.Lyrics = System.IO.File.ReadAllText(ofd.FileName);
                OnPropertyChanged("TagFile.Tag.Lyrics");
            }
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            VistaSaveFileDialog sfd = new VistaSaveFileDialog
            {
                Filter = string.Format("{0} (*.txt)|*.txt|{1} (*.*)|*.*", Application.Current.FindResource("TextFiles"), Application.Current.FindResource("AllFiles"))
            };
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
            TagFile.Tag.Genres = (from item in (List<Genre>) lstGenre.ItemsSource where item.IsChecked select item.Text).ToArray();
            TagFile.Save();
            await CurrentTrack.LoadInformation();
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
