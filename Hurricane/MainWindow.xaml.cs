using Hurricane.ViewModel;
using MahApps.Metro.Controls;

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
        }
    }
}