using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hurricane.Controls
{
    /// <summary>
    /// Interaktionslogik für RoundButton.xaml
    /// </summary>
    public partial class RoundButton : Button
    {
        public static readonly DependencyProperty ContentBrushProperty = DependencyProperty.Register(
            "ContentBrush", typeof (Brush), typeof (RoundButton), new PropertyMetadata(default(Brush)));

        public RoundButton()
        {
            InitializeComponent();
            
        }

        public Brush ContentBrush
        {
            get { return (Brush) GetValue(ContentBrushProperty); }
            set { SetValue(ContentBrushProperty, value); }
        }
    }
}
