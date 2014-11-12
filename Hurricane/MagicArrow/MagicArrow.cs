using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;

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

        void Application_Deactivated(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.Print("Deactivated");
            if(BaseWindow.ActualHeight == System.Windows.SystemParameters.WorkArea.Height && BaseWindow.Left == 0 && BaseWindow.Top == 0){
                //The window is at the right site
                MoveWindowOutOfScreen();
                System.Diagnostics.Debug.Print("Move Out");
            }
        }

        void window_Activated(object sender, EventArgs e)
        {
            if (MovedOut) { MoveWindowBackInScreen(); }
        }

        #region Animations
        Storyboard MoveWindowOutOfScreenStoryboard;
        protected void MoveWindowOutOfScreen()
        {
            if (MoveOut != null) MoveOut(this, EventArgs.Empty);
            if (MoveWindowOutOfScreenStoryboard == null)
            {
                MoveWindowOutOfScreenStoryboard = new Storyboard();
                DoubleAnimation outanimation = new DoubleAnimation();
                outanimation.From = 0;
                outanimation.To = -(BaseWindow.ActualWidth + 50); //For the shadow
                outanimation.Duration = TimeSpan.FromMilliseconds(150);

                MoveWindowOutOfScreenStoryboard.Children.Add(outanimation);
                Storyboard.SetTargetName(outanimation, BaseWindow.Name);
                Storyboard.SetTargetProperty(outanimation, new PropertyPath(Window.LeftProperty));
            }
            MovedOut = true;
            MoveWindowOutOfScreenStoryboard.Begin(BaseWindow);
            BaseWindow.Topmost = true;
            BaseWindow.ShowInTaskbar = false;
            StartMagic();
        }

        Storyboard MoveWindowBackInScreenStoryboard;
        protected void MoveWindowBackInScreen()
        {
            if (MoveWindowBackInScreenStoryboard == null)
            {
                MoveWindowBackInScreenStoryboard = new Storyboard();
                DoubleAnimation inanimation = new DoubleAnimation();
                inanimation.From = BaseWindow.Left;
                inanimation.To = 0;
                inanimation.Duration = TimeSpan.FromMilliseconds(150);

                MoveWindowBackInScreenStoryboard.Children.Add(inanimation);
                Storyboard.SetTargetName(inanimation, BaseWindow.Name);
                Storyboard.SetTargetProperty(MoveWindowBackInScreenStoryboard, new PropertyPath(Window.LeftProperty));
            }
            MovedOut = false;
            MoveWindowBackInScreenStoryboard.Begin(BaseWindow);
            BaseWindow.Activate();
            BaseWindow.ShowInTaskbar = true;
            StopMagic();
            BaseWindow.Topmost = false;
        }
        #endregion

        #region MagicPower
        protected void StartMagic()
        {
         Utilities.HookManager.MouseHook.HookManager.MouseMove += HookManager_MouseMove;
        }

        protected bool IsInZone;
        void HookManager_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.X < 5 && e.Y < System.Windows.SystemParameters.WorkArea.Height - 10) //That you can click on the startbutton
            {
                if (!IsInZone)
                {
                    if (Settings.HurricaneSettings.Instance.Config.DisableMagicArrowInGame && Utilities.WindowHelper.FullscreenWindowIsInForeground()) return;//Checks if there is a game in the foreground. its not so nice if you play a game and then comes a damn arrow inside your view :)
                    IsInZone = true;
                    ShowMagicArrow(e.Y);
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

        protected void StopMagic()
        {
            Utilities.HookManager.MouseHook.HookManager.MouseMove -= HookManager_MouseMove;
        }

        protected void ShowMagicArrow(int top)
        {
            if (!Settings.HurricaneSettings.Instance.Config.ShowMagicArrowBelowCursor)
            {
                if (top + 40 > System.Windows.SystemParameters.WorkArea.Height - 10)
                {
                    top -= 40;
                }
                else { top += 40; }
            }
            MagicWindow = new MagicArrowWindow(top);
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

        public void Register(Window window)
        {
            if (this.BaseWindow != null) throw new InvalidOperationException("Only one window can be registered");
            this.BaseWindow = window;
            Application.Current.Deactivated += Application_Deactivated;
            //window.Activated += window_Activated; dont works
            window.SourceInitialized += (s,e) => {
                HwndSource source = PresentationSource.FromVisual(BaseWindow) as HwndSource;
                source.AddHook(WndProc);
            };
        }

      const  int BringTheWindowToFrontMessage = 3532;
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

        public void Unregister()
        {
            Application.Current.Deactivated -= Application_Deactivated;
            this.BaseWindow = null;
            //window.Activated -= window_Activated;
        }

        public void Dispose()
        {
            this.StopMagic();
        }
    }
}
