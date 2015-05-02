using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Hurricane.Utilities;

namespace Hurricane.Controls
{
    public partial class MagicMetroWindow
    {
        private bool _isDragging; //If the window is currently dragging
        private bool _canRestore;

        private void WindowSkin_DragMoveStart(object sender, EventArgs e)
        {
            _isDragging = true;
            _canRestore = WindowState == WindowState.Maximized;
            ResizeMode = ResizeMode.CanResize;
            WindowHelper.DisableAeroSnap(new WindowInteropHelper(this).Handle);

            _magicArrow.DockManager.DragStart();

            if (CurrentView.Configuration.NeedsMovingHelp)
            {
                try
                {
                    DragMove();
                }
                catch (InvalidOperationException)
                {
                    //ignore
                }
            }
        }

        private void WindowSkin_DragMoveStop(object sender, EventArgs e)
        {
            _magicArrow.DockManager.DragStop();
        }

        void DockManager_DragStopped(object sender, EventArgs e)
        {
            _isDragging = false;
            ResizeMode = CurrentView.Configuration.IsResizable ? ResizeMode.CanResize : ResizeMode.NoResize;
        }

        private void WindowSkin_TitleBarMouseMove(object sender, MouseEventArgs e)
        {
            if (_canRestore)
            {
                _canRestore = false;

                var percentHorizontal = e.GetPosition(this).X / ActualWidth;
                var targetHorizontal = RestoreBounds.Width * percentHorizontal;

                var percentVertical = e.GetPosition(this).Y / ActualHeight;
                var targetVertical = RestoreBounds.Height * percentVertical;

                WindowState = WindowState.Normal;

                var mousePosition = Utilities.Cursor.Position;
                Left = mousePosition.X - targetHorizontal;
                Top = mousePosition.Y - targetVertical;

                try
                {
                    DragMove();
                }
                catch (InvalidOperationException)
                {
                }
            }
        }
    }
}
