using System.Windows;
using System.Windows.Media;

namespace Hurricane.Controls
{
    /// <summary>
    /// Interaktionslogik für PlayPauseButton.xaml
    /// </summary>
    public partial class PlayPauseButton
    {
        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(
            "IsPlaying", typeof (bool), typeof (PlayPauseButton), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty ContentBrushProperty = DependencyProperty.Register(
            "ContentBrush", typeof (Brush), typeof (PlayPauseButton), new PropertyMetadata(default(Brush)));

        public PlayPauseButton()
        {
            InitializeComponent();
        }

        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        public Brush ContentBrush
        {
            get { return (Brush)GetValue(ContentBrushProperty); }
            set { SetValue(ContentBrushProperty, value); }
        }
    }
}
