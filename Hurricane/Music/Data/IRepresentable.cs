using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Hurricane.Music.Data
{
    public interface IRepresentable
    {
        bool IsLoadingImage { get; set; }
        BitmapImage Image { get; set; }
    }
}
