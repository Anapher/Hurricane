using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Hurricane.Controls
{
    /// <summary>
    /// Interaktionslogik für PlayableImage.xaml
    /// </summary>
    public partial class PlayableImage
    {
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
            "Image", typeof (BitmapImage), typeof (PlayableImage), new PropertyMetadata(default(BitmapImage)));

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter", typeof (object), typeof (PlayableImage), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof (ICommand), typeof (PlayableImage), new PropertyMetadata(default(ICommand)));

        public PlayableImage()
        {
            InitializeComponent();
        }

        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public BitmapImage Image
        {
            get { return (BitmapImage) GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
    }
}
