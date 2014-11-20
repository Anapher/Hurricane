using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Hurricane.MagicArrow.DockManager;

namespace Hurricane.MagicArrow
{
    /// <summary>
    /// A class for the arrow that is displayed when the cursor hits the right Windows border.
    /// </summary>
    public class MagicArrow : IDisposable
    {
        public Window BaseWindow { get; set; }
        public bool MovedOut { get; set; }
        public MagicArrowWindow MagicWindow { get; set; }

        public event EventHandler MoveOut;
        public event DragEventHandler FilesDropped;

        #region Eventhandler
        void Application_Deactivated(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.Print("Deactivated");
            if (BaseWindow.ActualHeight == Utilities.WpfScreen.GetScreenFrom(new Point(BaseWindow.Left, 0)).WorkingArea.Height && (BaseWindow.Left == 0 || BaseWindow.Left == maxwidth -300) && BaseWindow.Top == 0)
            {
                //The window is at a good site
                MoveWindowOutOfScreen(BaseWindow.Left == 0 ? Side.Left : Side.Right);
                System.Diagnostics.Debug.Print("Move Out");
            }
        }
        #endregion

        #region Animations
        protected Side movedoutside;
        protected void MoveWindowOutOfScreen(Side side)
        {
            if (MoveOut != null) MoveOut(this, EventArgs.Empty);
            double newleft;
            if (side == Side.Left) { newleft = -(BaseWindow.ActualWidth + 50); } else { newleft = maxwidth + BaseWindow.ActualWidth + 50; }

            Storyboard MoveWindowOutOfScreenStoryboard = new Storyboard();
            DoubleAnimation outanimation = new DoubleAnimation();
            outanimation.From = BaseWindow.Left;
            outanimation.To = newleft;
            outanimation.Duration = TimeSpan.FromMilliseconds(150);
            outanimation.FillBehavior = FillBehavior.Stop;
            outanimation.Completed += (s, e) => { BaseWindow.Left = newleft; };
            MoveWindowOutOfScreenStoryboard.Children.Add(outanimation);
            Storyboard.SetTargetName(outanimation, BaseWindow.Name);
            Storyboard.SetTargetProperty(outanimation, new PropertyPath(Window.LeftProperty));

            MovedOut = true;
            MoveWindowOutOfScreenStoryboard.Begin(BaseWindow);
            //BaseWindow.Left = newleft;
            BaseWindow.ShowInTaskbar = false;
            StartMagic();
            movedoutside = side;
        }

        
        protected void MoveWindowBackInScreen()
        {
            double newleft;
            if (movedoutside == Side.Left) { newleft = 0; } else { newleft = maxwidth - BaseWindow.Width; }
            Storyboard MoveWindowBackInScreenStoryboard = new Storyboard();
            DoubleAnimation inanimation = new DoubleAnimation();
            inanimation.From = BaseWindow.Left;
            inanimation.To = newleft;
            inanimation.FillBehavior = FillBehavior.Stop;
            inanimation.Duration = TimeSpan.FromMilliseconds(150);
            inanimation.Completed += (s, e) => { BaseWindow.Topmost = false; BaseWindow.Left = newleft; };
            MoveWindowBackInScreenStoryboard.Children.Add(inanimation);
            Storyboard.SetTargetName(inanimation, BaseWindow.Name);
            Storyboard.SetTargetProperty(MoveWindowBackInScreenStoryboard, new PropertyPath(Window.LeftProperty));

            System.Diagnostics.Debug.Print("Move in: {0}", newleft);
            //BaseWindow.Left = newleft;
            MovedOut = false;
            BaseWindow.Topmost = true;
            BaseWindow.Activate();
            BaseWindow.ShowInTaskbar = true;
            MoveWindowBackInScreenStoryboard.Begin(BaseWindow);
            StopMagic();
        }
        #endregion

        #region Magic Arrow Showing
        protected void StopMagic()
        {
            Utilities.HookManager.MouseHook.HookManager.MouseMove -= HookManager_MouseMove;
        }

        protected void StartMagic()
        {
            Utilities.HookManager.MouseHook.HookManager.MouseMove += HookManager_MouseMove;
        }

        protected double maxwidth;
        protected bool IsInZone;
        protected bool ShowDogging = false;
        void HookManager_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (MovedOut)
            {
                if (((e.X < 5 && movedoutside == Side.Left) || (e.X > maxwidth -5 && movedoutside == Side.Right))  && e.Y < System.Windows.SystemParameters.WorkArea.Height - 10) //That you can click on the startbutton
                {
                    if (!IsInZone)
                    {
                        if (Settings.HurricaneSettings.Instance.Config.DisableMagicArrowInGame && Utilities.WindowHelper.FullscreenWindowIsInForeground()) return;//Checks if there is a game in the foreground. its not so nice if you play a game and then comes a damn arrow inside your view :)
                        IsInZone = true;
                        ShowMagicArrow(e.Y, e.X < 5 ? Side.Left : Side.Right);
                    }
                }
                else
                {
                    if (IsInZone)
                    {
                        IsInZone = false;
                        HideMagicArrow();
                    }
                }
            }
        }

        protected void ShowMagicArrow(int top, Side side)
        {
            if (!Settings.HurricaneSettings.Instance.Config.ShowMagicArrowBelowCursor)
            {
                if (top + 40 > System.Windows.SystemParameters.WorkArea.Height - 10)
                {
                    top -= 40;
                }
                else { top += 40; }
            }
            System.Diagnostics.Debug.Print("move in");
            MagicWindow = new MagicArrowWindow(top, side == Side.Left ? -10 : maxwidth, side == Side.Left ? 0 : maxwidth - 10, side);
            MagicWindow.MoveVisible += (s, e) =>
            {
                MoveWindowBackInScreen();
            };
            MagicWindow.Show();
            MagicWindow.FilesDropped += (s, e) => { if (this.FilesDropped != null) this.FilesDropped(this, e); };
        }

        protected void HideMagicArrow()
        {
            if (MagicWindow != null)
            {
                MagicWindow.Close();
                MagicWindow = null;
            }
        }
        #endregion

        #region Construction and Deconstruction
        public void Register(Window window)
        {
            if (this.BaseWindow != null) throw new InvalidOperationException("Only one window can be registered");
            this.BaseWindow = window;
            Application.Current.Deactivated += Application_Deactivated;

            window.SourceInitialized += (s, e) =>
            {
                HwndSource source = PresentationSource.FromVisual(BaseWindow) as HwndSource;
                source.AddHook(WndProc);
            };
            DockManager = new DockManager.DockManager(BaseWindow);
        }

        public MagicArrow()
        {
            maxwidth = 0;
            foreach (var screen in Utilities.WpfScreen.AllScreens())
                maxwidth += screen.WorkingArea.Width;
        }

        public void Unregister()
        {
            Application.Current.Deactivated -= Application_Deactivated;
            this.BaseWindow = null;
        }

        public void Dispose()
        {
            this.StopMagic();
            DockManager.Dispose();
            Unregister();
        }
        #endregion

        #region External Calls
        const int BringTheWindowToFrontMessage = 3532;
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case BringTheWindowToFrontMessage:
                    if (MovedOut) { MoveWindowBackInScreen(); }

                    System.Windows.Window mainwindow = Application.Current.MainWindow;
                    mainwindow.Topmost = true; //else the application wouldnt get focused
                    mainwindow.Activate();

                    mainwindow.Focus();
                    mainwindow.Topmost = false;
                    break;
            }
            return IntPtr.Zero;
        }
        #endregion

        #region DockSystem
        public DockManager.DockManager DockManager { get; set; }

        #endregion

    }

    public enum Side { Left, Right}
}
