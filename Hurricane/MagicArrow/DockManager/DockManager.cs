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
        protected double maxwidth;
        protected Window basewindow;
        public event EventHandler Undocked;
        public event EventHandler Docked;

        public DockManager(Window window)
        {
            maxwidth = WpfScreen.AllScreensWidth;
            basewindow = window;

            if (HurricaneSettings.Instance.Config.ApplicationState != null) CurrentSide = HurricaneSettings.Instance.Config.ApplicationState.CurrentSide;
        }

        public void DragStart()
        {
            if (!dragstopped) return;
            dragstopped = false;
            enabled = true;
            HookManager.MouseMove += HookManager_MouseMove;
        }

        private bool isatborder;
        private DockRangeWindow window;

        public double WindowHeight { get; set; }

        protected double left;
        protected DockingSide side = DockingSide.None; //new side
        public DockingSide CurrentSide { get; set; }//the applied side
        protected bool enabled;
        void HookManager_MouseMove(object sender, MouseEventArgs e)
        {
            if (!enabled) return;
            if (Mouse.LeftButton == MouseButtonState.Released) { DragStop(); return; } //If the user doubleclicks the window, relocates the window and releases the mouse, it doesn't get stopped

            if (e.X < 5 || e.X >= maxwidth - 5) // || !(basewindow.Left == 0 || basewindow.Left == maxwidth - 300)
            {
                if (!isatborder)
                {
                    isatborder = true;
                    side = e.X <= 5 ? DockingSide.Left : DockingSide.Right;
                    WindowHeight = WpfScreen.GetScreenFrom(new Point(e.X, e.Y)).WorkingArea.Height;
                    left = side == DockingSide.Left ? 0 : maxwidth - 300;
                    window = new DockRangeWindow(left, WindowHeight);
                    window.Show();
                }
            }
            else if (isatborder)
            {
                isatborder = false; CloseWindowIfExists(); side = DockingSide.None; if (Undocked != null) Undocked(this, EventArgs.Empty);
                Debug.Print("at border");
            }
        }

        protected void CloseWindowIfExists()
        {
            if (window != null) { window.Close(); window = null; }
        }

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
                basewindow.Left = CurrentSide == DockingSide.Left ? 0 : maxwidth - 300;
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
