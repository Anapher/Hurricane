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
            ((MainViewModel) DataContext).RefreshView += (s, e) => RefreshView();
        }
    }
}