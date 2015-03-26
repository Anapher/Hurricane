using System;
using System.Windows;
using System.Windows.Input;
using Hurricane.Settings;
using Hurricane.Utilities;
using Hurricane.Utilities.HookManager.MouseHook;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace Hurricane.MagicArrow.DockManager
{
    public class DockManager : IDisposable
    {
        #region Events

        public event EventHandler Undocked;
        public event EventHandler Docked;

        protected void OnUndocked()
        {
            if (Undocked != null) Undocked(this, EventArgs.Empty);
        }

        protected void OnDocked()
        {
            if (Docked != null) Docked(this, EventArgs.Empty);
        }

        #endregion

        #region Constructor

        private readonly Window _basewindow;
        public DockManager(Window window)
        {
            _basewindow = window;
            if (HurricaneSettings.Instance.CurrentState.ApplicationState != null) CurrentSide = HurricaneSettings.Instance.CurrentState.ApplicationState.CurrentSide;
        }

        #endregion

        #region Properties

        public bool IsEnabled { get; set; }

        #endregion

        public double WindowHeight { get; set; }

        private WindowPositionSide? _newSide;
        public WindowPositionSide? NewSide
        {
            get { return _newSide; }
            set { _newSide = value; }
        }

        public DockingSide CurrentSide { get; set; } //the applied side

        public void DragStart()
        {
            if (IsEnabled) return;
            IsEnabled = true;
            NewSide = null;
            HookManager.MouseMove += HookManager_MouseMove;
        }

        protected bool MouseIsLeftRightOrTop(int mouseX, int mouseY, out WindowPositionSide? side)
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

        protected bool WindowIsLeftOrRight()
        {
            return _basewindow.Left == WpfScreen.MostLeftX || (_basewindow.Left == WpfScreen.MostRightX - _basewindow.Width);
        }

        void HookManager_MouseMove(object sender, MouseEventArgs e)
        {
            //Side dock > top dock
            if (!IsEnabled) return;
            if (Mouse.LeftButton == MouseButtonState.Released) //If the user doubleclicks the window, relocates the window and releases the mouse, it doesn't get stopped
            {
                DragStop();
                return;
            }

            if (MouseIsLeftRightOrTop(e.X, e.Y, out _newSide))
            {
                var screen = WpfScreen.GetScreenFrom(new Point(e.X, e.Y));
                if (NewSide == WindowPositionSide.Left || NewSide == WindowPositionSide.Right)
                {
                    if (!IsAtRightOrLeftBorder)
                    {
                        IsAtRightOrLeftBorder = true;
                        OpenWindow(NewSide.Value, screen);
                        if (IsAtTop) IsAtTop = false;
                    }
                    return;
                }
                if (IsAtRightOrLeftBorder)
                {
                    IsAtRightOrLeftBorder = false;
                    CloseWindowIfExists();
                }
                if (NewSide == WindowPositionSide.Top)
                {
                    if (!IsAtTop || screen.DeviceName != DisplayingScreen)
                    {
                        IsAtTop = true;
                        OpenWindow(NewSide.Value, screen);
                        DisplayingScreen = screen.DeviceName;
                    }
                }
            }
            else
            {
                OnUndocked();
                if (IsAtTop)
                {
                    IsAtTop = false;
                    CloseWindowIfExists();
                }
                if (IsAtRightOrLeftBorder)
                {
                    IsAtRightOrLeftBorder = false;
                    CloseWindowIfExists();
                }
            }
        }

        #region DockWindow Management

        public bool IsAtRightOrLeftBorder { get; set; }
        public bool IsAtTop { get; set; }
        public string DisplayingScreen { get; set; }

        private DockRangeWindow _window;
        private void CloseWindowIfExists()
        {
            if (_window != null) { _window.Close(); _window = null; }
        }

        private void OpenWindow(WindowPositionSide side, WpfScreen screen)
        {
            if (side == WindowPositionSide.None) return;
            CloseWindowIfExists();

            double dockwindowLeft, dockwindowWidth;

            switch (side)
            {
                case WindowPositionSide.Left:
                    dockwindowLeft = WpfScreen.MostLeftX;
                    dockwindowWidth = 300;
                    break;
                case WindowPositionSide.Right:
                    dockwindowLeft = WpfScreen.MostRightX - 300;
                    dockwindowWidth = 300;
                    break;
                case WindowPositionSide.Top:
                    return;
                default:
                    throw new ArgumentOutOfRangeException("side");
            }

            _window = new DockRangeWindow(screen.WorkingArea.Top, dockwindowLeft, screen.WorkingArea.Height, dockwindowWidth);
            _window.Show();
        }

        #endregion

        public void DragStop()
        {
            if (!IsEnabled) return;
            IsEnabled = false;
            HookManager.MouseMove -= HookManager_MouseMove;
            CloseWindowIfExists();
            if (NewSide.HasValue)
            {
                switch (NewSide)
                {
                    case WindowPositionSide.Left:
                    case WindowPositionSide.Right:
                        CurrentSide = NewSide == WindowPositionSide.Left ? DockingSide.Left : DockingSide.Right;
                        ApplyCurrentSide();
                        if (Docked != null) Docked(this, EventArgs.Empty);
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
                WindowHeight = screen.WorkingArea.Height;
            }
        }

        public void Save()
        {
            if (HurricaneSettings.Instance.CurrentState.ApplicationState == null)
                HurricaneSettings.Instance.CurrentState.ApplicationState = new DockingApplicationState();
            HurricaneSettings.Instance.CurrentState.ApplicationState.CurrentSide = CurrentSide;
        }

        public void Dispose()
        {
            HookManager.MouseMove -= HookManager_MouseMove;
        }
    }

    public enum WindowPositionSide { Left, Right, Top, None }
}
