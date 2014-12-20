using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            maxwidth = Utilities.WpfScreen.AllScreensWidth;
            basewindow = window;

            if (Hurricane.Settings.HurricaneSettings.Instance.Config.ApplicationState != null) CurrentSide = Hurricane.Settings.HurricaneSettings.Instance.Config.ApplicationState.CurrentSide;
        }

        public void DragStart()
        {
            if (!dragstopped) return;
            dragstopped = false;
            enabled = true;
            Utilities.HookManager.MouseHook.HookManager.MouseMove += HookManager_MouseMove;
        }

        private bool isatborder = false;
        private DockRangeWindow window;

        public double WindowHeight { get; set; }

        protected double left;
        protected DockingSide side = DockingSide.None; //new side
        public DockingSide CurrentSide { get; set; }//the applied side
        protected bool enabled;
        void HookManager_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!enabled) return;
            if (System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Released) { DragStop(); return; } //If the user doubleclicks the window, relocates the window and releases the mouse, it doesn't get stopped

            if (e.X < 5 || e.X >= maxwidth - 5) // || !(basewindow.Left == 0 || basewindow.Left == maxwidth - 300)
            {
                if (!isatborder)
                {
                    isatborder = true;
                    side = e.X <= 5 ? DockingSide.Left : DockingSide.Right;
                    WindowHeight = Utilities.WpfScreen.GetScreenFrom(new System.Windows.Point(e.X, e.Y)).WorkingArea.Height;
                    left = side == DockingSide.Left ? 0 : maxwidth - 300;
                    window = new DockRangeWindow(left, WindowHeight);
                    window.Show();
                }
            }
            else if (isatborder)
            {
                isatborder = false; CloseWindowIfExists(); side = DockingSide.None; if (Undocked != null) Undocked(this, EventArgs.Empty);
                System.Diagnostics.Debug.Print("at border");
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
            Utilities.HookManager.MouseHook.HookManager.MouseMove -= HookManager_MouseMove;
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
            if (Settings.HurricaneSettings.Instance.Config.ApplicationState == null) Settings.HurricaneSettings.Instance.Config.ApplicationState = new DockingApplicationState();
            var appstate = Settings.HurricaneSettings.Instance.Config.ApplicationState;

            appstate.CurrentSide = CurrentSide;
        }

        public void Dispose()
        {
            Utilities.HookManager.MouseHook.HookManager.MouseMove -= HookManager_MouseMove;
        }
    }
}
