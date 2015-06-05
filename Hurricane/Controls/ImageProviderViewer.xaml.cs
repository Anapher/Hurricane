using System.Windows;
using Hurricane.Model.Music.Imagment;

namespace Hurricane.Controls
{
    /// <summary>
    /// Interaktionslogik für ImageProviderViewer.xaml
    /// </summary>
    public partial class ImageProviderViewer
    {
        public static readonly DependencyProperty ImageProviderProperty = DependencyProperty.Register(
            "ImageProvider", typeof (ImageProvider), typeof (ImageProviderViewer), new PropertyMetadata(default(ImageProvider)));

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof (FrameworkElement), typeof (ImageProviderViewer), new PropertyMetadata(default(FrameworkElement)));

        public ImageProviderViewer()
        {
            InitializeComponent();
        }

        public ImageProvider ImageProvider
        {
            get { return (ImageProvider)GetValue(ImageProviderProperty); }
            set { SetValue(ImageProviderProperty, value); }
        }

        public FrameworkElement Placeholder
        {
            get { return (FrameworkElement)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }
    }
}
