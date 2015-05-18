using System;
using System.Windows.Input;
using Hurricane.Model.Skin;

namespace Hurricane.WindowSkinNormal
{
    /// <summary>
    /// Interaktionslogik für WindowSkinNormal.xaml
    /// </summary>
    public partial class WindowSkinNormal : IWindowSkin
    {
        public WindowSkinNormal()
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
                    IsResizable = true,
                    MaxHeight = double.PositiveInfinity,
                    MaxWidth = double.PositiveInfinity,
                    MinHeight = 500,
                    MinWidth = 850,
                    ShowSystemMenuOnRightClick = true,
                    ShowWindowControls = true,
                    NeedsMovingHelp = true,
                    SupportsCustomBackground = true,
                    SupportsMinimizingToTray = true,
                    ShowFullscreenDialogs = true,
                    ShowTitleBar = false
                };

        private void Titlebar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleWindowState?.Invoke(this, EventArgs.Empty);
                return;
            }

            DragMoveStart?.Invoke(this, EventArgs.Empty);
        }

        private void Titlebar_OnMouseMove(object sender, MouseEventArgs e)
        {
            TitleBarMouseMove?.Invoke(this, e);
        }

        private void Titlebar_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DragMoveStop?.Invoke(this, EventArgs.Empty);
        }
    }
}
