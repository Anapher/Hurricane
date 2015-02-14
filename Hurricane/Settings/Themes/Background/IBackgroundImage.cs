using System.Windows.Media.Imaging;

namespace Hurricane.Settings.Themes.Background
{
    public interface IBackgroundImage
    {
        BitmapImage GetBackgroundImage();
        bool IsAnimated { get; }
        bool IsAvailable { get; }
        string DisplayText { get; }
    }
}