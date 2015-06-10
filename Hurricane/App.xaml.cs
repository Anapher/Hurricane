using System.Windows;
using Hurricane.ViewModel;

namespace Hurricane
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App
    {
        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = new MainWindow();
            await ((MainViewModel) mainWindow.DataContext).LoadData();
            mainWindow.Show();
        }
    }
}
