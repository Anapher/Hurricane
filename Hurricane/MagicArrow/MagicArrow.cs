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

        public event EventHandler MoveOut; //When moving out
        public event EventHandler MoveIn;
        public event DragEventHandler FilesDropped;

        private Utilities.ActiveWindowHook activewindowhook;

        #region Eventhandler
        void Application_Deactivated(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.Print("Deactivated");
            if (BaseWindow.ActualHeight == Utilities.WpfScreen.GetScreenFrom(new Point(BaseWindow.Left, 0)).WorkingArea.Height && (BaseWindow.Left == 0 || BaseWindow.Left == maxwidth - 300) && BaseWindow.Top == 0)
            {
                //The window is at a good site
                MoveWindowOutOfScreen(BaseWindow.Left == 0 ? Side.Left : Side.Right);
                System.Diagnostics.Debug.Print("Move Out");
            }
        }

        void activewindowhook_ActiveWindowChanged(object sender, IntPtr hwnd)
        {
            if (!MovedOut) return;
            if (Utilities.WindowHelper.WindowIsFullscreen(hwnd))
            {
                if (!strokewindow.IsInvisible) strokewindow.MoveInvisible();
                System.Diagnostics.Debug.Print("{0}: Its a fullscreen window",DateTime.Now.ToString());
            }
            else
            {
                if (strokewindow.IsInvisible) strokewindow.MoveVisible();
                System.Diagnostics.Debug.Print("{0}: It isnt a fullscreen window", DateTime.Now.ToString());
            }
            if (!strokewindow.IsInvisible)
            {
                strokewindow.Topmost = false;
                strokewindow.Topmost = true;
            }
        }
        #endregion

        #region Animations
        protected Side movedoutside;
        protected void MoveWindowOutOfScreen(Side side)
        {
            BaseWindow.Topmost = true;
            if (MoveOut != null) MoveOut(this, EventArgs.Empty);
            double newleft;
            if (side == Side.Left) { newleft = -(BaseWindow.ActualWidth + 50); } else { newleft = maxwidth + BaseWindow.ActualWidth + 50; }

            Storyboard MoveWindowOutOfScreenStoryboard = new Storyboard();
            DoubleAnimation outanimation = new DoubleAnimation();
            outanimation.From = BaseWindow.Left;
            outanimation.To = newleft;
            outanimation.Duration = TimeSpan.FromMilliseconds(150);
            outanimation.FillBehavior = FillBehavior.Stop;
            outanimation.Completed += (s, e) => { BaseWindow.Left = newleft; BaseWindow.Topmost = false; };
            MoveWindowOutOfScreenStoryboard.Children.Add(outanimation);
            Storyboard.SetTargetName(outanimation, BaseWindow.Name);
            Storyboard.SetTargetProperty(outanimation, new PropertyPath(Window.LeftProperty));

            MovedOut = true;
            MoveWindowOutOfScreenStoryboard.Begin(BaseWindow);
            BaseWindow.ShowInTaskbar = false;
            movedoutside = side;
            StartMagic();
        }

        protected void MoveWindowBackInScreen()
        {
            if (MoveIn != null) MoveIn(this, EventArgs.Empty);
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

            MovedOut = false;
            BaseWindow.Topmost = true;
            BaseWindow.Activate();
            BaseWindow.ShowInTaskbar = true;
            MoveWindowBackInScreenStoryboard.Begin(BaseWindow);
            StopMagic();
        }
        #endregion

        #region Magic Arrow Showing
        protected StrokeWindow strokewindow;
        protected void StopMagic()
        {
            if (strokewindow != null)
            {
                strokewindow.Close();
                strokewindow = null;
            }
            activewindowhook.Unhook();
        }

        protected void StartMagic()
        {
            if (movedoutside == Side.Left) { strokewindow = new StrokeWindow(Utilities.WpfScreen.GetScreenFrom(new Point(0, 0)).WorkingArea.Height, 0, movedoutside); } else { strokewindow = new StrokeWindow(Utilities.WpfScreen.GetScreenFrom(new Point(maxwidth, 0)).WorkingArea.Height, maxwidth, movedoutside); }
            strokewindow.Show();
            strokewindow.MouseMove += strokewindow_MouseMove;
            strokewindow.MouseLeave += strokewindow_MouseLeave;
            activewindowhook.Hook();
            activewindowhook.RaiseOne(); //If the current window is fullscreen, the event wouldn't be raised (because nothing changed)
            mousewasover = false;
        }

        protected bool MagicArrowIsShown = false;
        protected bool IsInZone = false;
        protected bool mousewasover = false;

        void strokewindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!MagicArrowIsShown && !IsInZone && StrokeWindow.PositionIsOk(movedoutside, System.Windows.Forms.Cursor.Position.X, 0, maxwidth))
            {
                IsInZone = true;
                Point p = e.GetPosition(strokewindow);
                ShowMagicArrow(p.Y, movedoutside);
            }
            mousewasover = true;
        }

        void strokewindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!MagicArrowIsShown)
            {
                HideMagicArrow();
                IsInZone = false;
            }
            else
            {
                int cursorX = System.Windows.Forms.Cursor.Position.X;
                if (movedoutside == Side.Left ? cursorX > 2 : cursorX < maxwidth - 3 || Utilities.WpfScreen.GetScreenFrom(new Point(cursorX, 0)).WorkingArea.Height > cursorX)
                    HideMagicArrow();
            }
        }

        protected double maxwidth;
        protected void ShowMagicArrow(double top, Side side)
        {
            MagicArrowIsShown = true;
            if (!Settings.HurricaneSettings.Instance.Config.ShowMagicArrowBelowCursor)
            {
                if (top + 40 > System.Windows.SystemParameters.WorkArea.Height - 10)
                {
                    top -= 40;
                }
                else { top += 40; }
            }
            MagicWindow = new MagicArrowWindow(top, side == Side.Left ? -10 : maxwidth, side == Side.Left ? 0 : maxwidth - 10, side);
            MagicWindow.MoveVisible += (s, e) =>
            {
                MoveWindowBackInScreen();
                MagicArrowIsShown = false;
                IsInZone = false;
                HideMagicArrow();
            };
            MagicWindow.MouseLeave += MagicWindow_MouseLeave;
            MagicWindow.Show();
            MagicWindow.FilesDropped += (s, e) => { if (this.FilesDropped != null) this.FilesDropped(this, e); };
            Task.Run(async () =>
            {
                while (MagicArrowIsShown)
                {
                    await Task.Delay(3000);
                    int cursorX = System.Windows.Forms.Cursor.Position.X;
                    if ((movedoutside == Side.Left && cursorX > 4) || (movedoutside == Side.Right && cursorX < maxwidth - 4)) Application.Current.Dispatcher.Invoke(() => HideMagicArrow());
                }
            });
        }

        void MagicWindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (StrokeWindow.PositionIsOk(movedoutside, System.Windows.Forms.Cursor.Position.X, 2, maxwidth))
            {
                strokewindow.SetLeft(movedoutside == Side.Left ? 0 : maxwidth - 1, movedoutside);
                HideMagicArrow();
            }
            else { strokewindow.SetLeft(movedoutside == Side.Left ? 0 : maxwidth - 1, movedoutside); }
        }

        protected void HideMagicArrow()
        {
            MagicArrowIsShown = false;
            IsInZone = false;
            if (MagicWindow != null && MagicWindow.Visibility == Visibility.Visible)
            {
                MagicWindow.MouseLeave -= MagicWindow_MouseLeave;
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
            activewindowhook = new Utilities.ActiveWindowHook();
            activewindowhook.ActiveWindowChanged += activewindowhook_ActiveWindowChanged;
        }

        public MagicArrow()
        {
            maxwidth = Utilities.WpfScreen.AllScreensWidth;
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
            activewindowhook.Dispose();
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

    public enum Side { Left, Right }
}