using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Hurricane.Settings;
using Hurricane.Utilities;
using Cursor = System.Windows.Forms.Cursor;

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

        private ActiveWindowHook _activewindowhook;

        #region Eventhandler
        void Application_Deactivated(object sender, EventArgs e)
        {
            if (BaseWindow.ActualHeight == WpfScreen.GetScreenFrom(new Point(BaseWindow.Left, 0)).WorkingArea.Height && (BaseWindow.Left == 0 || BaseWindow.Left == maxwidth - 300) && BaseWindow.Top == 0)
            {
                //The window is at a good site
                MoveWindowOutOfScreen(BaseWindow.Left == 0 ? Side.Left : Side.Right);
            }
        }

        void activewindowhook_ActiveWindowChanged(object sender, IntPtr hwnd)
        {
            if (!MovedOut) return;
            if (WindowHelper.WindowIsFullscreen(hwnd))
            {
                if (!Strokewindow.IsInvisible) Strokewindow.MoveInvisible();
                Debug.Print("{0}: Its a fullscreen window",DateTime.Now.ToString());
            }
            else
            {
                if (Strokewindow.IsInvisible) Strokewindow.MoveVisible();
                Debug.Print("{0}: It isnt a fullscreen window", DateTime.Now.ToString());
            }
            if (!Strokewindow.IsInvisible)
            {
                Strokewindow.Topmost = false;
                Strokewindow.Topmost = true;
            }
        }
        #endregion

        #region Animations
        protected Side _movedoutside;
        protected void MoveWindowOutOfScreen(Side side)
        {
            BaseWindow.Topmost = true;
            if (MoveOut != null) MoveOut(this, EventArgs.Empty);
            double newleft;
            if (side == Side.Left) { newleft = -(BaseWindow.ActualWidth + 50); } else { newleft = maxwidth + BaseWindow.ActualWidth + 50; }

            Storyboard moveWindowOutOfScreenStoryboard = new Storyboard();
            DoubleAnimation outanimation = new DoubleAnimation(BaseWindow.Left, newleft, TimeSpan.FromMilliseconds(150), FillBehavior.Stop);
            outanimation.Completed += (s, e) => { BaseWindow.Left = newleft; BaseWindow.Topmost = false; };
            moveWindowOutOfScreenStoryboard.Children.Add(outanimation);
            Storyboard.SetTargetName(outanimation, BaseWindow.Name);
            Storyboard.SetTargetProperty(outanimation, new PropertyPath(Window.LeftProperty));

            MovedOut = true;
            moveWindowOutOfScreenStoryboard.Begin(BaseWindow);
            BaseWindow.ShowInTaskbar = false;
            _movedoutside = side;
            StartMagic();
        }

        protected void MoveWindowBackInScreen()
        {
            if (MoveIn != null) MoveIn(this, EventArgs.Empty);
            double newleft;
            if (_movedoutside == Side.Left) { newleft = 0; } else { newleft = maxwidth - BaseWindow.Width; }
            Storyboard moveWindowBackInScreenStoryboard = new Storyboard();
            DoubleAnimation inanimation = new DoubleAnimation(BaseWindow.Left, newleft, TimeSpan.FromMilliseconds(150), FillBehavior.Stop);
            inanimation.Completed += (s, e) => { BaseWindow.Topmost = false; BaseWindow.Left = newleft; };
            moveWindowBackInScreenStoryboard.Children.Add(inanimation);
            Storyboard.SetTargetName(inanimation, BaseWindow.Name);
            Storyboard.SetTargetProperty(moveWindowBackInScreenStoryboard, new PropertyPath(Window.LeftProperty));

            MovedOut = false;
            BaseWindow.Topmost = true;
            BaseWindow.Activate();
            BaseWindow.ShowInTaskbar = true;
            moveWindowBackInScreenStoryboard.Begin(BaseWindow);
            StopMagic();
        }
        #endregion

        #region Magic Arrow Showing
        protected StrokeWindow Strokewindow;
        protected void StopMagic()
        {
            if (Strokewindow != null)
            {
                Strokewindow.Close();
                Strokewindow = null;
            }
            _activewindowhook.Unhook();
        }

        protected void StartMagic()
        {
            Strokewindow = _movedoutside == Side.Left ? new StrokeWindow(WpfScreen.GetScreenFrom(new Point(0, 0)).WorkingArea.Height, 0, _movedoutside) : new StrokeWindow(WpfScreen.GetScreenFrom(new Point(maxwidth, 0)).WorkingArea.Height, maxwidth, _movedoutside);
            Strokewindow.Show();
            Strokewindow.MouseMove += strokewindow_MouseMove;
            Strokewindow.MouseLeave += strokewindow_MouseLeave;
            Strokewindow.MouseDown += strokewindow_MouseDown;
            _activewindowhook.Hook();
            _activewindowhook.RaiseOne(); //If the current window is fullscreen, the event wouldn't be raised (because nothing changed)
            MouseWasOver = false;
        }

        void strokewindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MagicWindow != null)
            {
                var cursorposition = Cursor.Position;
                if (cursorposition.Y > MagicWindow.Top && cursorposition.Y < MagicWindow.Top + MagicWindow.Height && (_movedoutside == Side.Left ?  cursorposition.X < MagicWindow.Width : cursorposition.X > maxwidth - MagicWindow.Width))
                {
                    MoveWindowBackInScreen();
                    IsInZone = false;
                    HideMagicArrow();
                }
            }
        }

        protected bool MagicArrowIsShown;
        protected bool IsInZone;
        protected bool MouseWasOver;

        void strokewindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (!MagicArrowIsShown && !IsInZone && StrokeWindow.PositionIsOk(_movedoutside, Cursor.Position.X, 0, maxwidth))
            {
                IsInZone = true;
                Point p = e.GetPosition(Strokewindow);
                ShowMagicArrow(p.Y, _movedoutside);
            }
            MouseWasOver = true;
        }

        void strokewindow_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!MagicArrowIsShown)
            {
                HideMagicArrow();
                IsInZone = false;
            }
            else
            {
                int cursorX = Cursor.Position.X;
                if (_movedoutside == Side.Left ? cursorX > 2 : cursorX < maxwidth - 3 || WpfScreen.GetScreenFrom(new Point(cursorX, 0)).WorkingArea.Height > cursorX)
                    HideMagicArrow();
            }
        }

        protected double maxwidth;
        protected void ShowMagicArrow(double top, Side side)
        {

            MagicArrowIsShown = true;
            if (!HurricaneSettings.Instance.Config.ShowMagicArrowBelowCursor)
            {
                if (top + 40 > SystemParameters.WorkArea.Height - 10)
                {
                    top -= 40;
                }
                else { top += 40; }
            }
            MagicWindow = new MagicArrowWindow(top, side == Side.Left ? -10 : maxwidth, side == Side.Left ? 0 : maxwidth - 10, side);
            MagicWindow.MoveVisible += (s, e) =>
            {
                MoveWindowBackInScreen();
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
                    int cursorX = Cursor.Position.X;
                    if ((_movedoutside == Side.Left && cursorX > 4) || (_movedoutside == Side.Right && cursorX < maxwidth - 4)) Application.Current.Dispatcher.Invoke(HideMagicArrow);
                }
            });
        }

        void MagicWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            if (StrokeWindow.PositionIsOk(_movedoutside, Cursor.Position.X, 2, maxwidth))
            {
                Strokewindow.SetLeft(_movedoutside == Side.Left ? 0 : maxwidth - 1, _movedoutside);
                HideMagicArrow();
            }
            else { Strokewindow.SetLeft(_movedoutside == Side.Left ? 0 : maxwidth - 1, _movedoutside); }
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

            DockManager = new DockManager.DockManager(BaseWindow);
            _activewindowhook = new ActiveWindowHook();
            _activewindowhook.ActiveWindowChanged += activewindowhook_ActiveWindowChanged;
        }

        public MagicArrow()
        {
            maxwidth = WpfScreen.AllScreensWidth;
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
            _activewindowhook.Dispose();
            Unregister();
        }

        public void BringToFront()
        {
            if (MovedOut) { MoveWindowBackInScreen(); }

            Window mainwindow = Application.Current.MainWindow;
            mainwindow.Topmost = true; //else the application wouldnt get focused
            mainwindow.Activate();

            mainwindow.Focus();
            mainwindow.Topmost = false;
        }
        #endregion

        #region DockSystem
        public DockManager.DockManager DockManager { get; set; }

        #endregion

    }

    public enum Side { Left, Right }
}