using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Hurricane.Model.Skin;
using Hurricane.Utilities;
using Hurricane.Utilities.Hooks;

namespace Hurricane.MagicArrow
{
    /// <summary>
    /// A class for the arrow that is displayed when the cursor hits the right Windows border.
    /// </summary>
    internal class MagicArrow : IDisposable
    {
        private readonly ActiveWindowHook _activeWindowHook;
        private Side _movedOutSide;
        private bool _movedOut;

        public MagicArrow(Window window)
        {
            BaseWindow = window;
            Application.Current.Deactivated += Application_Deactivated;
            DockManager = new DockManager(window, DockingSide.Right);
            _activeWindowHook = new ActiveWindowHook();
            _activeWindowHook.ActiveWindowChanged += _activeWindowHook_ActiveWindowChanged;
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
                Application.Current.Deactivated -= Application_Deactivated;
                _activeWindowHook.Dispose();
                DockManager.DragStop();
                // free managed resources
            }
            // free native resources if there are any.
            _disposed = true;
        }

        ~MagicArrow()
        {
            Dispose(false);
        }

        public event EventHandler MoveOut;
        public event EventHandler MoveIn;

        public Window BaseWindow { get; set; }
        public DockManager DockManager { get; set; }

        void _activeWindowHook_ActiveWindowChanged(object sender, IntPtr hwnd)
        {

        }

        void Application_Deactivated(object sender, EventArgs e)
        {
            var first = DockManager.CurrentSide != DockingSide.None; //We make sure that the window is docked
            var secound = _movedOutSide == Side.Left
                ? BaseWindow.Left >= WpfScreen.MostLeftX
                : BaseWindow.Left <= WpfScreen.MostRightX;

            if (first && secound)
                //The window is at a good site
                MoveWindowOutOfScreen(DockManager.DockingSideToSide(DockManager.CurrentSide));
        }

        private async void MoveWindowOutOfScreen(Side side)
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

            _movedOut = true;
            BaseWindow.ShowInTaskbar = false;
            _movedOutSide = side;
            StartMagic();
        }

        private void MoveWindowBackInScreen()
        {
            if (MoveIn != null) MoveIn(this, EventArgs.Empty);
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
            if (Strokewindow != null)
            {
                Strokewindow.Close();
                Strokewindow = null;
            }

            _activeWindowHook.Disable();
        }
    }

    public enum Side { Left, Right }
}