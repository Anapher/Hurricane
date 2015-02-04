using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Hurricane.MagicArrow.DockManager;
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
            var first = DockManager.CurrentSide != DockingSide.None;
            var secound = _movedoutside == Side.Left
                ? BaseWindow.Left >= WpfScreen.MostLeftX
                : BaseWindow.Left <= WpfScreen.MostRightX;
            if (first && secound) //(BaseWindow.ActualHeight == WpfScreen.GetScreenFrom(new Point(BaseWindow.Left, 0)).WorkingArea.Height && (BaseWindow.Left == 0 || BaseWindow.Left == maxwidth - 300) && BaseWindow.Top == 0)
            {
                //The window is at a good site
                MoveWindowOutOfScreen(BaseWindow.Left == WpfScreen.MostLeftX ? Side.Left : Side.Right);
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
        protected async void MoveWindowOutOfScreen(Side side)
        {
            BaseWindow.Topmost = true;
            if (MoveOut != null) MoveOut(this, EventArgs.Empty);
            if (side == Side.Left)
            {
                for (var i = 0; i > -32; i--)
                {
                    await Task.Delay(1);
                    BaseWindow.Left = i * 10 + WpfScreen.MostLeftX;
                }
            }
            else
            {
                for (int i = 0; i < 32; i++)
                {
                    await Task.Delay(1);
                    BaseWindow.Left = WpfScreen.MostRightX - BaseWindow.Width + i * 10;
                }
            }

            MovedOut = true;
            BaseWindow.ShowInTaskbar = false;
            _movedoutside = side;
            StartMagic();

        }

        protected void MoveWindowBackInScreen()
        {
            if (MoveIn != null) MoveIn(this, EventArgs.Empty);
            double newleft;
            if (_movedoutside == Side.Left) { newleft = WpfScreen.MostLeftX; } else { newleft = WpfScreen.MostRightX - BaseWindow.Width; }
            BaseWindow.Left = _movedoutside == Side.Left ? newleft + 10 : newleft - 10;

            Storyboard moveWindowBackInScreenStoryboard = new Storyboard();
            DoubleAnimation inanimation = new DoubleAnimation(BaseWindow.Left, newleft, TimeSpan.FromMilliseconds(150), FillBehavior.Stop);
            inanimation.Completed += (s, e) => { BaseWindow.Topmost = false; BaseWindow.Left = newleft; };
            moveWindowBackInScreenStoryboard.Children.Add(inanimation);
            Storyboard.SetTargetName(inanimation, BaseWindow.Name);
            Storyboard.SetTargetProperty(moveWindowBackInScreenStoryboard, new PropertyPath(Window.LeftProperty));

            moveWindowBackInScreenStoryboard.Begin(BaseWindow);

            MovedOut = false;
            BaseWindow.Topmost = true;
            BaseWindow.Activate();
            BaseWindow.ShowInTaskbar = true;
            
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
            var screen = GetScreenFromSide(_movedoutside);
            Strokewindow = new StrokeWindow(screen.WorkingArea.Height, _movedoutside == Side.Left ? WpfScreen.MostLeftX : WpfScreen.MostRightX, screen.WorkingArea.Top, _movedoutside);
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
                if (cursorposition.Y > MagicWindow.Top && cursorposition.Y < MagicWindow.Top + MagicWindow.Height && (_movedoutside == Side.Left ? cursorposition.X < MagicWindow.Width + WpfScreen.MostLeftX : cursorposition.X > WpfScreen.MostRightX - MagicWindow.Width))
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
            if (!MagicArrowIsShown && !IsInZone && StrokeWindow.PositionIsOk(_movedoutside, Cursor.Position.X, WpfScreen.MostLeftX - 2, WpfScreen.MostRightX))
            {
                IsInZone = true;
                Point p = e.GetPosition(Strokewindow);
                var screen = WpfScreen.GetScreenFrom(p);
                ShowMagicArrow(p.Y + screen.WorkingArea.Top, _movedoutside);
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
                if (_movedoutside == Side.Left ? cursorX > 2 - WpfScreen.MostLeftX : cursorX < WpfScreen.MostRightX - 3 || WpfScreen.GetScreenFrom(new Point(cursorX, 0)).WorkingArea.Height > cursorX)
                    HideMagicArrow();
            }
        }

        protected void ShowMagicArrow(double top, Side side)
        {

            MagicArrowIsShown = true;
            if (!HurricaneSettings.Instance.Config.ShowMagicArrowBelowCursor)
            {
                if (top + 40 > GetScreenFromSide(_movedoutside).WorkingArea.Height - 10)
                {
                    top -= 40;
                }
                else { top += 40; }
            }
            MagicWindow = new MagicArrowWindow(top, side == Side.Left ? WpfScreen.MostLeftX - 10 : WpfScreen.MostRightX, side == Side.Left ? WpfScreen.MostLeftX : WpfScreen.MostRightX - 10, side);
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
                    await Task.Delay(1000);
                    int cursorX = Cursor.Position.X;
                    if ((_movedoutside == Side.Left && cursorX > 4 - WpfScreen.MostLeftX) || (_movedoutside == Side.Right && cursorX < WpfScreen.MostRightX - 4)) Application.Current.Dispatcher.Invoke(HideMagicArrow);
                }
            });
        }

        void MagicWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            if (StrokeWindow.PositionIsOk(_movedoutside, Cursor.Position.X, 2 - WpfScreen.MostLeftX, WpfScreen.MostRightX))
            {
                if (Strokewindow != null)
                    Strokewindow.SetLeft(_movedoutside == Side.Left ? WpfScreen.MostLeftX : WpfScreen.MostRightX - 1, _movedoutside);
                HideMagicArrow();
            }
            else { Strokewindow.SetLeft(_movedoutside == Side.Left ? WpfScreen.MostLeftX : WpfScreen.MostRightX - 1, _movedoutside); }
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

        protected WpfScreen GetScreenFromSide(Side side)
        {
            return
                WpfScreen.GetScreenFrom(side == Side.Left
                    ? new Point(WpfScreen.MostLeftX, 0)
                    : new Point(WpfScreen.MostRightX, 0));
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

        public void Unregister()
        {
            Application.Current.Deactivated -= Application_Deactivated;
            BaseWindow = null;
        }

        public void Dispose()
        {
            StopMagic();
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