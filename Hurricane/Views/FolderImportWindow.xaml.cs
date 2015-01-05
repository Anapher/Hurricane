using System;
using System.ComponentModel;
using System.Windows;
using MahApps.Metro.Controls;
using WPFFolderBrowser;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for FolderImportWindow.xaml
    /// </summary>
    public partial class FolderImportWindow : MetroWindow, INotifyPropertyChanged
    {
        public FolderImportWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WPFFolderBrowserDialog fbd = new WPFFolderBrowserDialog
            {
                Title = Application.Current.Resources["SelectedFolder"].ToString(),
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
            };
            if (fbd.ShowDialog() == true)
                SelectedPath = fbd.FileName;
        }

        private string _selectedpath;
        public string SelectedPath
        {
            get { return _selectedpath; }
            set
            {
                if (value != _selectedpath)
                {
                    _selectedpath = value;
                    if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("SelectedPath"));
                }
            }
        }

        public bool IncludeSubfolder { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
