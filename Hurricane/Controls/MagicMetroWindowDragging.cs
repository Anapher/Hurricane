using System;
using System.Windows;
using System.Windows.Input;

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
            //ResizeMode = ResizeMode.CanResize;
            //WindowHelper.DisableAeroSnap(new WindowInteropHelper(this).Handle);
            //ResizeMode = ResizeMode.NoResize;

            _magicArrow.DockManager.DragStart();

            if (CurrentView.Configuration.NeedsMovingHelp)
            {
                try
                {
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                        Application.Current.Dispatcher.Invoke(DragMove);
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

        private void DockManager_DragStopped(object sender, EventArgs e)
        {
            _isDragging = false;
            //if (!CurrentView.Configuration.IsResizable) return; //Cant resize anyway
            //ResizeMode = ResizeMode.CanResize;
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
