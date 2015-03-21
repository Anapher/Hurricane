using Hurricane.Music;
using Hurricane.ViewModels;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for QueueManagerWindow.xaml
    /// </summary>
    public partial class QueueManagerWindow
    {
        public QueueManager QueueManager { get; set; }

        public QueueManagerWindow(QueueManager queueManager)
        {
            DataContext = new QueueManagerViewModel(queueManager);
            InitializeComponent();
        }
    }
}
