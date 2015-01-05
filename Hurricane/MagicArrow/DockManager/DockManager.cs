using System;
using System.Diagnostics;
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

        protected Window basewindow;
        public DockManager(Window window)
        {
            basewindow = window;
            if (HurricaneSettings.Instance.Config.ApplicationState != null) CurrentSide = HurricaneSettings.Instance.Config.ApplicationState.CurrentSide;
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

        protected bool MouseIsLeftRightOrTop(int MouseX, int MouseY, out WindowPositionSide? side)
        {
            if (MouseX < 5)
            {
                side = WindowPositionSide.Left;
                return true;
            }
            if (MouseX >= WpfScreen.AllScreensWidth - 5)
            {
                side = WindowPositionSide.Right;
                return true;
            }
            if (MouseY < 5)
            {
                side = WindowPositionSide.Top;
                return true;
            }
            side = WindowPositionSide.None;
            return false;
        }

        protected bool WindowIsLeftOrRight()
        {
            return basewindow.Left == 0 || (basewindow.Left == WpfScreen.AllScreensWidth - basewindow.Width);
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

        private DockRangeWindow window;
        private void CloseWindowIfExists()
        {
            if (window != null) { window.Close(); window = null; }
        }

        private void OpenWindow(WindowPositionSide side, WpfScreen screen)
        {
            if (side == WindowPositionSide.None) return;
            CloseWindowIfExists();

            double dockwindowLeft, dockwindowWidth;

            switch (side)
            {
                case WindowPositionSide.Left:
                    dockwindowLeft = 0;
                    dockwindowWidth = 300;
                    break;
                case WindowPositionSide.Right:
                    dockwindowLeft = WpfScreen.AllScreensWidth - 300;
                    dockwindowWidth = 300;
                    break;
                case WindowPositionSide.Top:
                    dockwindowLeft = screen.WorkingArea.Left;
                    dockwindowWidth = screen.WorkingArea.Width;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("side");
            }

            window = new DockRangeWindow(0, dockwindowLeft, screen.WorkingArea.Height, dockwindowWidth);
            window.Show();
        }
        #endregion


        protected bool dragstopped = true;

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
                        basewindow.WindowState = WindowState.Maximized;
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
                basewindow.Top = 0;
                basewindow.Left = CurrentSide == DockingSide.Left ? 0 : WpfScreen.AllScreensWidth - 300;
                WindowHeight = WpfScreen.GetScreenFrom(new Point(basewindow.Left, 0)).WorkingArea.Height;
            }
        }

        public void Save()
        {
            if (HurricaneSettings.Instance.Config.ApplicationState == null)
                HurricaneSettings.Instance.Config.ApplicationState = new DockingApplicationState();
            HurricaneSettings.Instance.Config.ApplicationState.CurrentSide = CurrentSide;
        }

        public void Dispose()
        {
            HookManager.MouseMove -= HookManager_MouseMove;
        }
    }

    public enum WindowPositionSide { Left, Right, Top, None }
}
