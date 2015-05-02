namespace Hurricane.Views.Docking
{
    /// <summary>
    /// Interaction logic for MagicArrowWindow.xaml
    /// </summary>
    public partial class DockPlaceholderWindow
    {
        public DockPlaceholderWindow(double left, double height) : this(0, left, height, 300)
        {
        }

        public DockPlaceholderWindow(double top, double left, double height, double width)
        {
            InitializeComponent();
            Top = top;
            Left = left;
            Height = height;
            Width = width;
        }
    }
}