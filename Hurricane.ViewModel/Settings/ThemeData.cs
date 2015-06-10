using System.Windows;

namespace Hurricane.ViewModel.Settings
{
    public class ThemeData
    {
        public string Name => Application.Current.Resources[Key].ToString();
        public string Key { get; set; }
    }
}