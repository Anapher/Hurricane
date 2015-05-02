using System;
using System.Windows;
using System.Windows.Input;
using Hurricane.Model.Skin;
using Hurricane.Utilities;
using Hurricane.Utilities.Hooks;
using Hurricane.Views.Docking;

namespace Hurricane.MagicArrow
{
    class DockManager
    {
        private readonly Window _basewindow;
        private WindowPositionSide? _newSide;
        private readonly MouseHook _mouseHook;
        private bool _isAtRightOrLeftBorder;
        private bool _isAtTop;
        private DockPlaceholderWindow _placeholderWindow;
        private string _displaingScreen;
        private bool _firedUndocked;

        public DockManager(Window window, DockingSide dockingSide)
        {
            _basewindow = window;
            CurrentSide = dockingSide;
            _mouseHook = new MouseHook();
            _mouseHook.MouseMove += MouseHookOnMouseMove;
        }

        public event EventHandler Undocked;
        public event EventHandler Docked;
        public event EventHandler DragStopped;

        public DockingSide CurrentSide { get; set; } //the applied side
        public bool IsDragging { get; set; }

        public void DragStart()
        {
            if (IsDragging) return;
            IsDragging = true;
            _newSide = null;
            _mouseHook.Enable();
        }

        public void DragStop()
        {
            if (!IsDragging) return;
            IsDragging = false;
            _mouseHook.Disable();
            CloseWindow();
            OnDragStopped();
            if (_newSide.HasValue)
            {
                switch (_newSide.Value)
                {
                    case WindowPositionSide.Left:
                    case WindowPositionSide.Right:
                        CurrentSide = _newSide == WindowPositionSide.Left ? DockingSide.Left : DockingSide.Right;
                        ApplyCurrentSide();
                        OnDocked();
                        return;
                    case WindowPositionSide.Top:
                        _basewindow.WindowState = WindowState.Maximized;
                        break;
                    case WindowPositionSide.None:
                        break;
                }
            }
            CurrentSide = DockingSide.None;
        }

        public void ApplyCurrentSide()
        {
            if (CurrentSide == DockingSide.Left || CurrentSide == DockingSide.Right)
            {
                _basewindow.Left = CurrentSide == DockingSide.Left ? WpfScreen.MostLeftX : WpfScreen.MostRightX - 300;
                var screen = WpfScreen.GetScreenFrom(new Point(_basewindow.Left, 0));
                _basewindow.Top = screen.WorkingArea.Top;
                _basewindow.Height = screen.WorkingArea.Height;
            }
        }

        public static Side DockingSideToSide(DockingSide dockingSide)
        {
            switch (dockingSide)
            {
                case DockingSide.Left:
                    return Side.Left;
                case DockingSide.Right:
                    return Side.Right;
                default:
                    throw new ArgumentOutOfRangeException("dockingSide");
            }
        }

        private void OnUndocked()
        {
            if (Undocked != null) Undocked(this, EventArgs.Empty);
        }

        private void OnDocked()
        {
            if (Docked != null) Docked(this, EventArgs.Empty);
        }

        private void OnDragStopped()
        {
            if (DragStopped != null) DragStopped(this, EventArgs.Empty);
        }

        private void MouseHookOnMouseMove(object sender, MouseMoveEventArgs e)
        {
            if (!IsDragging || Mouse.LeftButton == MouseButtonState.Released)
                return;

            if (MouseIsLeftRightOrTop(e.X, e.Y, out _newSide))
            {
                var screen = WpfScreen.GetScreenFrom(new Point(e.X, e.Y));
                if (_newSide == WindowPositionSide.Left || _newSide == WindowPositionSide.Right)
                {
                    if (!_isAtRightOrLeftBorder)
                    {
                        _isAtRightOrLeftBorder = true;
                        OpenWindow(_newSide.Value, screen);
                        if (_isAtTop) _isAtTop = false;
                    }
                    return;
                }

                if (_isAtRightOrLeftBorder)
                {
                    _isAtRightOrLeftBorder = false;
                    CloseWindow();
                }

                if (_newSide == WindowPositionSide.Top)
                {
                    if (!_isAtTop || screen.DeviceName != _displaingScreen)
                    {
                        _isAtTop = true;
                        _displaingScreen = screen.DeviceName;
                    }
                }
            }
            else
            {
                if (!_firedUndocked)
                {
                    _firedUndocked = true;
                    OnUndocked();
                }
                if (_isAtTop)
                {
                    _isAtTop = false;
                    CloseWindow();
                }
                if (_isAtRightOrLeftBorder)
                {
                    _isAtRightOrLeftBorder = false;
                    CloseWindow();
                }
            }
        }

        private bool MouseIsLeftRightOrTop(int mouseX, int mouseY, out WindowPositionSide? side)
        {
            if (mouseX < WpfScreen.MostLeftX + 5)
            {
                side = WindowPositionSide.Left;
                return true;
            }
            if (mouseX >= WpfScreen.MostRightX - 5)
            {
                side = WindowPositionSide.Right;
                return true;
            }
            if (mouseY < 5)
            {
                side = WindowPositionSide.Top;
                return true;
            }
            side = WindowPositionSide.None;
            return false;
        }

        private void CloseWindow()
        {
            if (_placeholderWindow != null && _placeholderWindow.IsLoaded)
            {
                _placeholderWindow.Close();
                _placeholderWindow = null;
            }
        }

        private void OpenWindow(WindowPositionSide side, WpfScreen screen)
        {
            if (side == WindowPositionSide.None || side == WindowPositionSide.Top) return;
            CloseWindow();

            double dockwindowLeft;
            switch (side)
            {
                case WindowPositionSide.Left:
                    dockwindowLeft = WpfScreen.MostLeftX;
                    break;
                case WindowPositionSide.Right:
                    dockwindowLeft = WpfScreen.MostRightX - 300;
                    break;
                case WindowPositionSide.Top:
                    return;
                default:
                    throw new ArgumentOutOfRangeException("side");
            }

            _placeholderWindow = new DockPlaceholderWindow(screen.WorkingArea.Top, dockwindowLeft, screen.WorkingArea.Height, 300);
            _placeholderWindow.Show();
        }
    }

    public enum WindowPositionSide { Left, Right, Top, None }
}
