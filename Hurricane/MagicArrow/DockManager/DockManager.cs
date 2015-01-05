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

        private bool isatborder;

        public double WindowHeight { get; set; }

        protected double left;
        protected DockingSide side = DockingSide.None; //new side
        public DockingSide CurrentSide { get; set; }//the applied side
        protected bool enabled;
        private bool FullscreenWindowIsOpen;

        public void DragStart()
        {
            if (!dragstopped) return;
            dragstopped = false;
            enabled = true;
            HookManager.MouseMove += HookManager_MouseMove;
        }

        protected bool MouseIsLeftOrRight(int MouseX)
        {
            return MouseX < 5 || MouseX >= WpfScreen.AllScreensWidth - 5;
        }

        protected bool WindowIsLeftOrRight()
        {
            return basewindow.Left == 0 || (basewindow.Left == WpfScreen.AllScreensWidth - basewindow.Width);
        }

        void HookManager_MouseMove(object sender, MouseEventArgs e)
        {
            if (!enabled) return;
            if (Mouse.LeftButton == MouseButtonState.Released) //If the user doubleclicks the window, relocates the window and releases the mouse, it doesn't get stopped
            {
                DragStop();
                return;
            }
            
            if (MouseIsLeftOrRight(e.X)) // || !(basewindow.Left == 0 || basewindow.Left == maxwidth - 300)
            {
                if (!isatborder)
                {
                    isatborder = true;
                    side = e.X <= 5 ? DockingSide.Left : DockingSide.Right;
                    WindowHeight = WpfScreen.GetScreenFrom(new Point(e.X, e.Y)).WorkingArea.Height;
                    left = side == DockingSide.Left ? 0 : WpfScreen.AllScreensWidth - 300;
                    window = new DockRangeWindow(left, WindowHeight);
                    window.Show();
                    return;
                }
            }
            else if (isatborder) //&& !WindowIsLeftOrRight()
            {
                isatborder = false;
                CloseWindowIfExists();
                Debug.Print("close window");
                side = DockingSide.None;
                OnUndocked();
                return;
            }

            if (e.Y < 1 && !FullscreenWindowIsOpen && basewindow.WindowState != WindowState.Maximized)
            {
                var currentScreen = WpfScreen.GetScreenFrom(new Point(e.X, 0));
                window = new DockRangeWindow(0, 0, currentScreen.WorkingArea.Height, currentScreen.WorkingArea.Width);
                window.Show();
                FullscreenWindowIsOpen = true;
            }
            else if (FullscreenWindowIsOpen)
            {
                CloseWindowIfExists();
                FullscreenWindowIsOpen = false;
            }
        }

        #region DockWindow Management

        private DockRangeWindow window;
        private void CloseWindowIfExists()
        {
            if (window != null) { window.Close(); window = null; }
        }

        private void OpenWindow()
        {
            
        }
        #endregion


        protected bool dragstopped = true;
        public void DragStop()
        {
            if (dragstopped) return;
            dragstopped = true;
            enabled = false;
            HookManager.MouseMove -= HookManager_MouseMove;
            CurrentSide = side;

            if (side != DockingSide.None)
            {
                basewindow.Left = left;
                basewindow.Top = 0;
                if (Docked != null) Docked(this, EventArgs.Empty);
            }
            side = DockingSide.None;
            CloseWindowIfExists();
        }

        public void ApplyCurrentSide()
        {
            if (CurrentSide == DockingSide.Left || CurrentSide == DockingSide.Right)
            {
                basewindow.Top = 0;
                basewindow.Left = CurrentSide == DockingSide.Left ? 0 : WpfScreen.AllScreensWidth - 300;
                isatborder = true;
            }
            else { isatborder = false; }
        }

        public void Save()
        {
            if (HurricaneSettings.Instance.Config.ApplicationState == null) HurricaneSettings.Instance.Config.ApplicationState = new DockingApplicationState();
            var appstate = HurricaneSettings.Instance.Config.ApplicationState;

            appstate.CurrentSide = CurrentSide;
        }

        public void Dispose()
        {
            HookManager.MouseMove -= HookManager_MouseMove;
        }
    }
}
