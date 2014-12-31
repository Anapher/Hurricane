using MahApps.Metro.Controls;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : MetroWindow
    {
        public ProgressWindow(string title, bool indeterminate)
        {
            InitializeComponent();
            this.Title = title;
            this.prg.IsIndeterminate = indeterminate;
        }

        public void SetText(string text)
        {
          this.txtinfo.Text = text;
        }

        public void SetProgress(double progress)
        {
            prg.Value = progress;
        }

        public void SetTitle(string title)
        {
            this.Title = title;
        }
    }
}
