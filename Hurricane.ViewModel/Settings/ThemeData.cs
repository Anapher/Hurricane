using System.Windows;
using MahApps.Metro;

namespace Hurricane.ViewModel.Settings
{
    public class ThemeData
    {
        public AppTheme Theme { get; set; }
        public string Name => Application.Current.Resources[Key].ToString();
        public string Key { get; set; }
    }
}