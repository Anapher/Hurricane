using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Hurricane.Music.Track;
using Hurricane.ViewModelBase;
using Hurricane.Views;
using TagLib;
using Microsoft.Win32;

namespace Hurricane.ViewModels
{
    public class TagEditorViewModel : PropertyChangedBase
    {
        public File TagFile { get; set; }
        public LocalTrack Track { get; set; }

        private readonly Window _baseWindow;

        public TagEditorViewModel(LocalTrack track, Window baseWindow)
        {
            TagFile = File.Create(track.Path);
            Track = track;
            baseWindow.Closed += (s, e) => TagFile.Dispose();
            _baseWindow = baseWindow;

            AllGenres = Genres.Audio.ToList();
            AllGenres.AddRange(Enum.GetValues(typeof(Genre)).Cast<Genre>().Select(PlayableBase.GenreToString).Where(x => !AllGenres.Contains(x)));
            AllGenres.Sort();

            SelectedGenres = track.Genres.Select(PlayableBase.GenreToString).ToList();
        }

        public TagEditorViewModel()
        {
            
        }

        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(parameter => _baseWindow.Close())); }
        }

        private RelayCommand _saveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(async parameter =>
                {
                    TagFile.Tag.Genres = SelectedGenres.ToArray();
                    try
                    {
                        TagFile.Save();
                    }
                    catch (Exception ex)
                    {
                        var message =
                            new MessageWindow(
                                string.Format(Application.Current.Resources["SaveTagsError"].ToString(), ex.Message),
                                Application.Current.Resources["Error"].ToString(), false) {Owner = _baseWindow};
                        message.ShowDialog();
                        return;
                    }

                    await Track.LoadInformation();
                    _baseWindow.Close();
                }));
            }
        }

        private RelayCommand _openLyrics;
        public RelayCommand OpenLyrics
        {
            get
            {
                return _openLyrics ?? (_openLyrics = new RelayCommand(parameter =>
                {
                    var ofd = new OpenFileDialog()
                    {
                        Filter = string.Format("{0} (*.txt)|*.txt|{1} (*.*)|*.*", Application.Current.FindResource("TextFiles"), Application.Current.FindResource("AllFiles"))
                    };
                    if (ofd.ShowDialog() == true)
                    {
                        TagFile.Tag.Lyrics = System.IO.File.ReadAllText(ofd.FileName);
                        OnPropertyChanged("TagFile");
                    }
                }));
            }
        }

        private RelayCommand _saveLyricsAs;
        public RelayCommand SaveLyricsAs
        {
            get
            {
                return _saveLyricsAs ?? (_saveLyricsAs = new RelayCommand(parameter =>
                {
                    var sfd = new SaveFileDialog()
                    {
                        Filter = string.Format("{0} (*.txt)|*.txt|{1} (*.*)|*.*", Application.Current.FindResource("TextFiles"), Application.Current.FindResource("AllFiles"))
                    };
                    if (sfd.ShowDialog() == true)
                    {
                        System.IO.File.WriteAllText(sfd.FileName, TagFile.Tag.Lyrics);
                    }
                }));
            }
        }

        private RelayCommand _clearLyrics;
        public RelayCommand ClearLyrics
        {
            get
            {
                return _clearLyrics ?? (_clearLyrics = new RelayCommand(parameter =>
                {
                    TagFile.Tag.Lyrics = string.Empty;
                    OnPropertyChanged("TagFile");
                }));
            }
        }

        private RelayCommand _refresValues;
        public RelayCommand RefreshValues
        {
            get
            {
                return _refresValues ?? (_refresValues = new RelayCommand(parameter =>
                {
                    SelectedValues = string.Join(", ", SelectedGenres);
                }));
            }
        }

        public List<string> AllGenres { get; set; }

        private List<string> _selectedGenres;
        public List<string> SelectedGenres
        {
            get { return _selectedGenres; }
            set
            {
                SetProperty(value, ref _selectedGenres);
            }
        }

        private string _selectedValues;
        public string SelectedValues
        {
            get { return _selectedValues; }
            set
            {
                SetProperty(value, ref _selectedValues);
            }
        }
    }
}