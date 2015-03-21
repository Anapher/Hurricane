namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow
    {
        public ProgressWindow(string title, bool indeterminate)
        {
            InitializeComponent();
            Title = title;
            StatusProgressBar.IsIndeterminate = indeterminate;
        }

        public void SetText(string text)
        {
            InfoTextBlock.Text = text;
        }

        public void SetProgress(double progress)
        {
            StatusProgressBar.Value = progress;
        }

        public void SetTitle(string title)
        {
            Title = title;
        }
    }
}
