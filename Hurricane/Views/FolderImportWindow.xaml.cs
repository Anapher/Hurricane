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
    /// Interaction logic for FolderImportWindow.xaml
    /// </summary>
    public partial class FolderImportWindow : MahApps.Metro.Controls.MetroWindow, System.ComponentModel.INotifyPropertyChanged
    {
        public FolderImportWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog fbd = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.MyMusic;
            fbd.ShowNewFolderButton = false;
            fbd.Description = System.Windows.Application.Current.FindResource("selectfolder").ToString();
            fbd.UseDescriptionForTitle = true;
            if (fbd.ShowDialog() == true)
            {
                SelectedPath = fbd.SelectedPath;
            }
        }

        private string selectedpath;
        public string SelectedPath
        {
            get { return selectedpath; }
            set
            {
                if (value != selectedpath)
                {
                    selectedpath = value;
                    if (PropertyChanged != null) PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("SelectedPath"));
                }
            }
        }

        public bool IncludeSubfolder { get; set; }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
