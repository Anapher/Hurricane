using System.Windows;
using Hurricane.Model.Music.TrackProperties;

namespace Hurricane.Controls
{
    /// <summary>
    /// Interaktionslogik für ImageProviderViewer.xaml
    /// </summary>
    public partial class ImageProviderViewer
    {
        public static readonly DependencyProperty ImageProviderProperty = DependencyProperty.Register(
            "ImageProvider", typeof (ImageProvider), typeof (ImageProviderViewer), new PropertyMetadata(default(ImageProvider)));

        public ImageProviderViewer()
        {
            InitializeComponent();
        }

        public ImageProvider ImageProvider
        {
            get { return (ImageProvider)GetValue(ImageProviderProperty); }
            set { SetValue(ImageProviderProperty, value); }
        }
    }
}
