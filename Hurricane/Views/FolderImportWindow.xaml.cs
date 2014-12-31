using System;
using System.ComponentModel;
using System.Windows;
using MahApps.Metro.Controls;
using Ookii.Dialogs.Wpf;

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
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.MyMusic,
                ShowNewFolderButton = false,
                Description = Application.Current.FindResource("SelectedFolder").ToString(),
                UseDescriptionForTitle = true
            };
            if (fbd.ShowDialog() == true)
                SelectedPath = fbd.SelectedPath;
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
