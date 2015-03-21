using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Hurricane.Music.Track;
using Microsoft.Win32;
using TagLib;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for TagEditorWindow.xaml
    /// </summary>
    public partial class TagEditorWindow : INotifyPropertyChanged
    {
        public TagEditorWindow(LocalTrack track)
        {
            TagFile = File.Create(track.Path);
            CurrentTrack = track;
            InitializeComponent();

            List<Genre> genres = Genres.Audio.Select(item => new Genre(item, TagFile.Tag.Genres.Contains(item))).ToList();
            GenreListBox.ItemsSource = genres;
        }

        public LocalTrack CurrentTrack { get; set; }
        public File TagFile { get; set; }

        #region Lyrics
        private void MenuItemOpenLyrics_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
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
            var sfd = new SaveFileDialog()
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
            LyrcisTextBox.Clear();
        }
        #endregion

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            TagFile.Tag.Genres = (from item in (List<Genre>)GenreListBox.ItemsSource where item.IsChecked select item.Text).ToArray();
            try
            {
                TagFile.Save();
            }
            catch (Exception ex)
            {
                MessageWindow message =
                    new MessageWindow(
                        string.Format(Application.Current.Resources["SaveTagsError"].ToString(), ex.Message),
                        Application.Current.Resources["Error"].ToString(), false) {Owner = this};
                message.ShowDialog();
                return;
            }

            await CurrentTrack.LoadInformation();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            TagFile.Dispose();
        }

        #region INotifyPropertyChanged
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public class Genre
        {
            public string Text { get; set; }
            public bool IsChecked { get; set; }

            public Genre(string genre, bool ischecked)
            {
                Text = genre;
                IsChecked = ischecked;
            }
        }
    }
}
