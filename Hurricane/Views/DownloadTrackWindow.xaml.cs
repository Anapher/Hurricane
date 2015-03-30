using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Exceptionless.Validation;
using Hurricane.Music.Download;
using Hurricane.Settings;
using Microsoft.Win32;
using WPFFolderBrowser;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for ConverterSettingsWindow.xaml
    /// </summary>
    public partial class DownloadTrackWindow : INotifyPropertyChanged
    {   
        public DownloadSettings DownloadSettings { get; set; }

        private readonly bool _isInFileMode;
        private readonly string _defaultExtension;

        /// <summary>
        /// Dowload track
        /// </summary>
        /// <param name="trackName">The name of the trac without extension</param>
        /// <param name="defaultExtension">The default audio extension in the format .{extension} (example: .mp3)</param>
        public DownloadTrackWindow(string trackName, string defaultExtension)
        {
            InitializeComponent();
            _isInFileMode = true;
            _defaultExtension = defaultExtension;
            SelectedPath = Path.Combine(HurricaneSettings.Instance.Config.DownloadSettings.DownloadFolder,
                trackName + defaultExtension);
            CheckIfFileExists();

            DownloadSettings = HurricaneSettings.Instance.Config.DownloadSettings;
            OnPropertyChanged("DownloadSettings");
        }

        /// <summary>
        /// Download tracks
        /// </summary>
        public DownloadTrackWindow()
        {
            InitializeComponent();

            _isInFileMode = false;
            SelectedPath = HurricaneSettings.Instance.Config.DownloadSettings.DownloadFolder;
            CanAccept = true;

            DownloadSettings = HurricaneSettings.Instance.Config.DownloadSettings;
            OnPropertyChanged("DownloadSettings");
            Title = Application.Current.Resources["DownloadTracks"].ToString();
        }

        private void SelectPath_Click(object sender, RoutedEventArgs e)
        {
            if (_isInFileMode)
            {
                var sfd = new SaveFileDialog
                {
                    Filter =
                        string.Format("{0}|{1}|MP3|*.mp3|AAC|*.aac|WMA|*.wma",
                            Application.Current.Resources["CopyFromOriginal"], "*" + _defaultExtension),
                    FilterIndex = (int)(DownloadSettings.Format +1),
                    FileName = Path.GetFileName(SelectedPath)
                };
                if (sfd.ShowDialog(this) == true)
                {
                    SelectedPath = sfd.FileName;
                    DownloadSettings.Format = (AudioFormat)(sfd.FilterIndex -1);
                    if (sfd.FilterIndex > 1)
                        DownloadSettings.IsConverterEnabled = true;
                    OnPropertyChanged("DownloadSettings");
                    CheckIfFileExists();
                }
            }
            else
            {
                var fbd = new WPFFolderBrowserDialog {InitialDirectory = DownloadSettings.DownloadFolder};
                if (fbd.ShowDialog(this) == true)
                    SelectedPath = fbd.FileName;
            }
        }

        private void SetError(TextBox textBox, string error)
        {
            var textBoxBindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
            if (textBoxBindingExpression == null) return;
            ValidationError validationError =
                new ValidationError(new RequiredValidationRule(),
                    textBoxBindingExpression) {ErrorContent = error};


            Validation.MarkInvalid(
                textBoxBindingExpression,
                validationError);
        }

        private string _selectedPath;
        public string SelectedPath
        {
            get { return _selectedPath; }
            set
            {
                _selectedPath = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInFileMode)
            {
                var directory = new FileInfo(SelectedPath).DirectoryName;
                if (!string.IsNullOrEmpty(directory))
                    DownloadSettings.DownloadFolder = directory;
            }
            else
            {
                DownloadSettings.DownloadFolder = SelectedPath;
            }
            DialogResult = true;
        }

        private bool _canAccept;
        public bool CanAccept
        {
            get { return _canAccept; }
            set
            {
                _canAccept = value;
                OnPropertyChanged();
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInFileMode) return;
            if (string.IsNullOrEmpty(SelectedPath)) return;
            var selectedPath = new FileInfo(SelectedPath);

            SelectedPath =
                Path.Combine(selectedPath.DirectoryName,
                    Path.GetFileNameWithoutExtension(selectedPath.FullName) + DownloadSettings.GetExtension(_defaultExtension));

            CheckIfFileExists();
        }

        private void CheckIfFileExists()
        {
            if (!_isInFileMode) return;
            if (string.IsNullOrEmpty(SelectedPath))
            {
                CanAccept = false;
                return;
            }
            if (File.Exists(SelectedPath))
            {
                SetError(PathTextBox, Application.Current.Resources["FileAlreadyExists"].ToString());
                CanAccept = false;
            }
            else
            {
                var expression = PathTextBox.GetBindingExpression(TextBox.TextProperty);
                if (expression != null)
                    Validation.ClearInvalid(expression);
                CanAccept = true;
            }
        }
    }
}
