using System;
using System.Threading.Tasks;
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
            "ImageProvider", typeof (ImageProvider), typeof (ImageProviderViewer), new PropertyMetadata(default(ImageProvider), ImageProviderPropertyChangedCallback));

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof (FrameworkElement), typeof (ImageProviderViewer), new PropertyMetadata(default(FrameworkElement)));

        public static readonly DependencyProperty HidePlaceholderAtBeginningProperty = DependencyProperty.Register(
            "HidePlaceholderAtBeginning", typeof (bool), typeof (ImageProviderViewer), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty HighPriorityImageProperty = DependencyProperty.Register(
            "HighPriorityImage", typeof (bool), typeof (ImageProviderViewer), new PropertyMetadata(default(bool)));

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

        public bool HidePlaceholderAtBeginning
        {
            get { return (bool)GetValue(HidePlaceholderAtBeginningProperty); }
            set { SetValue(HidePlaceholderAtBeginningProperty, value); }
        }

        public bool HighPriorityImage
        {
            get { return (bool)GetValue(HighPriorityImageProperty); }
            set { SetValue(HighPriorityImageProperty, value); }
        }

        private async static void ImageProviderPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var imageProvider = dependencyObject as ImageProviderViewer;
            if (imageProvider == null)
                throw new ArgumentException(nameof(dependencyObject));

            if (!imageProvider.HidePlaceholderAtBeginning || imageProvider.Placeholder == null)
                return;

            var newImageProvider = dependencyPropertyChangedEventArgs.NewValue as ImageProvider;
            if (newImageProvider == null)
                return;

            newImageProvider.BeginLoadingImage(imageProvider.HighPriorityImage);

            imageProvider.PlaceholderControl.Visibility = Visibility.Hidden;
            await Task.Delay(500);
            imageProvider.PlaceholderControl.Visibility = Visibility.Visible;
        }
    }
}
