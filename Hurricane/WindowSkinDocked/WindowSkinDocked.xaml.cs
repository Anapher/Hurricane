using System;
using System.Windows.Input;
using Hurricane.Model.Skin;
using Hurricane.Utilities;

namespace Hurricane.WindowSkinDocked
{
    /// <summary>
    /// Interaktionslogik für WindowSkinDocked.xaml
    /// </summary>
    public partial class WindowSkinDocked : IWindowSkin
    {
        public WindowSkinDocked()
        {
            InitializeComponent();
        }

        public event EventHandler DragMoveStart;
        public event EventHandler DragMoveStop;
        public event EventHandler CloseRequest;
        public event EventHandler ToggleWindowState;
        public event EventHandler<MouseEventArgs> TitleBarMouseMove;

        public WindowSkinConfiguration Configuration
            =>
                new WindowSkinConfiguration
                {
                    IsResizable = false,
                    MaxHeight = WpfScreen.MaxHeight,
                    MinHeight = 400,
                    MaxWidth = 300,
                    MinWidth = 300,
                    NeedsMovingHelp = true,
                    ShowFullscreenDialogs = false,
                    ShowSystemMenuOnRightClick = false,
                    ShowTitleBar = false,
                    ShowWindowControls = false,
                    SupportsCustomBackground = false,
                    SupportsMinimizingToTray = false
                };

        private void Titlebar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMoveStart?.Invoke(this, EventArgs.Empty);
        }

        private void Titlebar_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DragMoveStop?.Invoke(this, EventArgs.Empty);
        }
    }
}
