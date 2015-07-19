using System.Windows;
using System.Windows.Media.Animation;
using Hurricane.ViewModel;

namespace Hurricane
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var viewModel = (MainViewModel) DataContext;
            viewModel.RefreshView += (s, e) => RefreshView();
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof (Timeline),
                new FrameworkPropertyMetadata {DefaultValue = 60}
                );
        }
    }
}