using System;
using System.Windows.Media.Imaging;

namespace Hurricane.Music.Data
{
    public interface IRepresentable
    {
        bool IsLoadingImage { get; set; }
        BitmapImage Image { get; set; }
        event EventHandler ImageLoadedComplete;
    }
}
