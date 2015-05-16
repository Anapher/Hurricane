using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Hurricane.Model.Skin;
using Hurricane.Utilities;
using Hurricane.Utilities.Hooks;
using Hurricane.Views.Docking;
using Cursor = Hurricane.Utilities.Cursor;

namespace Hurricane.MagicArrow
{
    /// <summary>
    /// A class for the arrow that is displayed when the cursor hits the right Windows border.
    /// </summary>
    internal class MagicArrowService : IDisposable
    {
        private readonly ActiveWindowHook _activeWindowHook;
        private Side _movedOutSide; //The side where the window moved out
        private bool _movedOut; //If the window is moved out
        private MagicTriggerWindow _magicTrigger; //The trigger window
        private MagicArrowWindow _magicArrow;
        private bool _isInZone;
        private readonly DispatcherTimer _magicArrowCheckTimer;

        public MagicArrowService(Window window)
        {
            BaseWindow = window;
            Application.Current.Deactivated += Application_Deactivated;
            DockManager = new DockManager(window, DockingSide.Left);
            _activeWindowHook = new ActiveWindowHook();
            _activeWindowHook.ActiveWindowChanged += ActiveWindowHook_ActiveWindowChanged;
            _magicArrowCheckTimer = new DispatcherTimer{Interval = TimeSpan.FromSeconds(1)};
            _magicArrowCheckTimer.Tick += MagicArrowCheckTimer_Tick;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        public virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                Debug.Print("MagicArrow already disposed!");
                return;
            }

            if (disposing)
            {
                StopMagic();
                Application.Current.Deactivated -= Application_Deactivated;
                _activeWindowHook.Dispose();
                DockManager.DragStop();
                // free managed resources
            }
            // free native resources if there are any.
            _disposed = true;
        }

        ~MagicArrowService()
        {
            Dispose(false);
        }

        public event EventHandler MoveOut;
        public event EventHandler MoveIn;

        public Window BaseWindow { get; set; }
        public DockManager DockManager { get; set; }
        public bool IsMagicArrowVisible { get; private set; }

        private async void MoveWindowOutOfScreen(Side side)
        {
            BaseWindow.Topmost = true;
            MoveOut?.Invoke(this, EventArgs.Empty);
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

            _movedOut = true;
            BaseWindow.ShowInTaskbar = false;
            _movedOutSide = side;
            StartMagic();
        }

        private void MoveWindowBackInScreen()
        {
            MoveIn?.Invoke(this, EventArgs.Empty);
            double newleft;
            if (_movedOutSide == Side.Left) { newleft = WpfScreen.MostLeftX; } else { newleft = WpfScreen.MostRightX - BaseWindow.Width; }
            BaseWindow.Left = _movedOutSide == Side.Left ? newleft + 10 : newleft - 10;

            var animation = new DoubleAnimation(BaseWindow.Left, newleft, TimeSpan.FromMilliseconds(150),
                FillBehavior.Stop) {EasingFunction = new CircleEase()};

            animation.Completed += (s, e) => { BaseWindow.Topmost = false; BaseWindow.Left = newleft; };
            BaseWindow.BeginAnimation(Window.LeftProperty, animation);

            _movedOut = false;
            BaseWindow.Topmost = true;
            BaseWindow.Activate();
            BaseWindow.ShowInTaskbar = true;

            StopMagic();
        }

        private void StopMagic()
        {
            if (_magicTrigger != null && _magicTrigger.IsLoaded)
            {
                _magicTrigger.Close();
                _magicTrigger = null;
            }
            _activeWindowHook.Disable();
        }

        private void StartMagic()
        {
            Debug.Print("MagicArrow: Start");
            var screen = GetScreenFromSide(_movedOutSide);
            _magicTrigger = new MagicTriggerWindow(screen.WorkingArea.Height, _movedOutSide == Side.Left ? WpfScreen.MostLeftX : WpfScreen.MostRightX, screen.WorkingArea.Top, _movedOutSide);
            _magicTrigger.Show();
            _magicTrigger.MouseMove += MagicTriggerOnMouseMove;
            _magicTrigger.MouseLeave += MagicTriggerOnMouseLeave;
            _magicTrigger.MouseDown += MagicTriggerOnMouseDown;
            _activeWindowHook.Enable();
            OnActiveWindowChanged(WindowHelper.GetForegroundWindow());//If the current window is fullscreen, the event wouldn't be raised (because nothing changed)
        }

        private void MagicTriggerOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (IsMagicArrowVisible)
            {
                var cursorposition = Cursor.Position;
                if (cursorposition.Y > _magicArrow.Top && cursorposition.Y < _magicArrow.Top + _magicArrow.Height && (_movedOutSide == Side.Left ? cursorposition.X < _magicArrow.Width + WpfScreen.MostLeftX : cursorposition.X > WpfScreen.MostRightX - _magicArrow.Width))
                {
                    MoveWindowBackInScreen();
                    _isInZone = false;
                    HideMagicArrow();
                }
            }
        }

        private void MagicTriggerOnMouseLeave(object sender, MouseEventArgs e)
        {
            Trace.WriteLine("MagicTrigger: mouse leave");
            if (!IsMagicArrowVisible)
            {
                HideMagicArrow();
                _isInZone = false;
            }
            else
            {
                if (!_magicArrow.IsMouseOver)
                {
                    HideMagicArrow();
                }
            }
        }

        private void MagicTriggerOnMouseMove(object sender, MouseEventArgs e)
        {
            Trace.WriteLine("MagicTrigger: mouse move");
            if (!IsMagicArrowVisible && !_isInZone && PositionIsOk(_movedOutSide, Cursor.Position.X, WpfScreen.MostLeftX - 2, WpfScreen.MostRightX))
            {
                _isInZone = true;
                var p = e.GetPosition(_magicTrigger);
                var screen = WpfScreen.GetScreenFrom(p);
                ShowMagicArrow(p.Y + screen.WorkingArea.Top, _movedOutSide);
            }
        }

        private void MagicArrowOnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            Trace.WriteLine("MagicArrow: mouse leave");
            if (PositionIsOk(_movedOutSide, Cursor.Position.X, 2 - WpfScreen.MostLeftX, WpfScreen.MostRightX))
            {
                _magicTrigger?.SetLeft(_movedOutSide == Side.Left ? WpfScreen.MostLeftX : WpfScreen.MostRightX - 1, _movedOutSide);
                HideMagicArrow();
            }
            else { _magicTrigger.SetLeft(_movedOutSide == Side.Left ? WpfScreen.MostLeftX : WpfScreen.MostRightX - 1, _movedOutSide); }
        }

        private void ActiveWindowHook_ActiveWindowChanged(object sender, IntPtr hwnd)
        {
            if (!_movedOut) return;
            OnActiveWindowChanged(hwnd);
        }

        private void OnActiveWindowChanged(IntPtr handle)
        {
            if (WindowHelper.IsWindowFullscreen(handle))
            {
                if (_magicTrigger.IsMagicTriggerVisible) _magicTrigger.IsMagicTriggerVisible = false;
            }
            else
            {
                if (!_magicTrigger.IsMagicTriggerVisible) _magicTrigger.IsMagicTriggerVisible = true;
            }

            if (_magicTrigger.IsMagicTriggerVisible)
            {
                _magicTrigger.Topmost = false;
                _magicTrigger.Topmost = true;
            }
        }

        private void Application_Deactivated(object sender, EventArgs e)
        {
            var first = DockManager.CurrentSide != DockingSide.None; //We make sure that the window is docked
            var secound = _movedOutSide == Side.Left
                ? BaseWindow.Left >= WpfScreen.MostLeftX
                : BaseWindow.Left <= WpfScreen.MostRightX;

            if (first && secound)
                //The window is at a good site
                MoveWindowOutOfScreen(DockManager.DockingSideToSide(DockManager.CurrentSide));
        }

        private void MagicArrowCheckTimer_Tick(object sender, EventArgs e)
        {
            Trace.WriteLine("MagicArrow: Check");
            var cursorX = Cursor.Position.X;
            if (((_movedOutSide == Side.Left && cursorX > 4 - WpfScreen.MostLeftX) ||
                (_movedOutSide == Side.Right && cursorX < WpfScreen.MostRightX - 4)) && !_magicArrow.IsMouseOver)
            {
                Application.Current.Dispatcher.Invoke(HideMagicArrow);
            }
        }

        private void ShowMagicArrow(double top, Side side)
        {
            Trace.WriteLine("MagicArrow: Show");
            IsMagicArrowVisible = true;
            _magicArrow = new MagicArrowWindow(top, side == Side.Left ? WpfScreen.MostLeftX : WpfScreen.MostRightX - 10, side);
            _magicArrow.Click += (s, e) =>
            {
                MoveWindowBackInScreen();
                _isInZone = false;
                HideMagicArrow();
            };
            _magicArrow.MouseLeave += MagicArrowOnMouseLeave;
            _magicArrow.Show();
            _magicArrowCheckTimer.Start();
        }

        private void HideMagicArrow()
        {
            Trace.WriteLine("MagicArrow: Hide");
            IsMagicArrowVisible = false;
            _isInZone = false;
            _magicArrowCheckTimer.Stop();
            if (_magicArrow != null && _magicArrow.IsLoaded)
            {
                _magicArrow.MouseLeave -= MagicArrowOnMouseLeave;
                _magicArrow.Close();
                _magicArrow = null;
            }
        }

        private static WpfScreen GetScreenFromSide(Side side)
        {
            return
                WpfScreen.GetScreenFrom(side == Side.Left
                    ? new Point(WpfScreen.MostLeftX, 0)
                    : new Point(WpfScreen.MostRightX, 0));
        }

        private static bool PositionIsOk(Side side, double position, double width, double screenwidth)
        {
            return side == Side.Left ? position >= width : position <= screenwidth - width - 1;
        }
    }

    public enum Side { Left, Right }
}