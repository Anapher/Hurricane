using System.Windows;
using System.Windows.Media;
using MahApps.Metro;

namespace Hurricane.ViewModel.Settings
{
    public class AccentColorData
    {
        public string Key { get; set; }
        public Brush ColorBrush { get; set; }
        public string Name => Application.Current.Resources[Key].ToString();
        public Accent AccentColor { get; set; }
    }
}