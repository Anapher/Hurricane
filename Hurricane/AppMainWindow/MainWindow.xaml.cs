using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using CSCore.SoundOut;
using Hardcodet.Wpf.TaskbarNotification;
using Hurricane.AppMainWindow.Messages;
using Hurricane.AppMainWindow.WindowSkins;
using Hurricane.MagicArrow.DockManager;
using Hurricane.Music.CustomEventArgs;
using Hurricane.Settings;
using Hurricane.Utilities;
using Hurricane.Utilities.Native;
using Hurricane.ViewModels;
using Hurricane.Views;
using MahApps.Metro.Controls;
using Microsoft.Win32;

// ReSharper disable once CheckNamespace
namespace Hurricane
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MagicArrow.MagicArrow MagicArrow { get; set; }
        public bool IsInSmartMode { get; set; }
        public IWindowSkin HostedWindow { get; set; }

        private IWindowSkin _smartWindowSkin;
        public IWindowSkin SmartWindowSkin { get { return _smartWindowSkin ?? (_smartWindowSkin = new WindowSmartView()); } }

        private IWindowSkin _advancedWindowSkin;
        public IWindowSkin AdvancedWindowSkin { get { return _advancedWindowSkin ?? (_advancedWindowSkin = new WindowAdvancedView()); } }

        #region Constructor & Load

        public MainWindow()
        {
            InitializeComponent();
            HostedWindow = null;
            MagicArrow = new MagicArrow.MagicArrow();
            MagicArrow.MoveOut += (s, e) => { HideEqualizer(); HostedWindow.DisableWindow(); };
            MagicArrow.MoveIn += (s, e) => { HostedWindow.EnableWindow(); };
            MagicArrow.FilesDropped += (s, e) => { MainViewModel.Instance.DragDropFiles((string[])e.Data.GetData(DataFormats.FileDrop)); };
            MagicArrow.Register(this);

            Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;
            StateChanged += MainWindow_StateChanged;

            MagicArrow.DockManager.Docked += (s, e) => { ApplyHostWindow(SmartWindowSkin); };
            MagicArrow.DockManager.Undocked += (s, e) =>
            {
                ApplyHostWindow(AdvancedWindowSkin);
            };

            var appsettings = HurricaneSettings.Instance.CurrentState;
            if (appsettings.ApplicationState == null)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                appsettings.ApplicationState = new DockingApplicationState
                {
                    CurrentSide = DockingSide.None,
                    Height = 600,
                    Width = 1000,
                    Left = Left,
                    Top = Top
                };
            }

            if (appsettings.ApplicationState.CurrentSide == DockingSide.None)
            {
                if (appsettings.ApplicationState.Left < WpfScreen.MostRightX) //To prevent that the window is out of view when the user unplugs a monitor
                {
                    Height = appsettings.ApplicationState.Height;
                    Width = appsettings.ApplicationState.Width;
                    Left = appsettings.ApplicationState.Left;
                    Top = appsettings.ApplicationState.Top;
                    WindowState = appsettings.ApplicationState.WindowState;
                }
                else
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }

            MagicArrow.DockManager.CurrentSide = appsettings.ApplicationState.CurrentSide;
            WindowDialogService = new WindowDialogService(this);
            SystemEvents.PowerModeChanged += SystemEventsOnPowerModeChanged;
        }

        public void CenterWindowOnScreen()
        {
            var screen = WpfScreen.GetScreenFrom(this).WorkingArea;
            Left = (screen.Width / 2) - (Width / 2);
            Top = (screen.Height / 2) - (Height / 2);
        }

        public void RefreshHostWindow(bool saveInformation)
        {
            if (MagicArrow.DockManager.CurrentSide == DockingSide.None)
            {
                ApplyHostWindow(AdvancedWindowSkin, saveInformation);
            }
            else
            {
                ApplyHostWindow(SmartWindowSkin, saveInformation);
                Height = WpfScreen.GetScreenFrom(new Point(Left, 0)).WorkingArea.Height;
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MagicArrow.DockManager.ApplyCurrentSide();

                var viewmodel = MainViewModel.Instance;
                viewmodel.Loaded(this);

                RefreshHostWindow(false);

                viewmodel.MusicManager.CSCoreEngine.PlaybackStateChanged += CSCoreEngine_PlaybackStateChanged;

                ResetFlyout();
            }
            catch (Exception ex)
            {
#if (!DEBUG)
                var reportExceptionWindow = new ReportExceptionWindow(ex) { Owner = this };
                reportExceptionWindow.ShowDialog();
#else
                MessageBox.Show(ex.ToString());
#endif
            }
        }

        #endregion

        #region ApplyHostWindow

        protected void ApplyHostWindow(IWindowSkin skin, bool saveinformation = true)
        {
            if (skin == HostedWindow) return;
            if (HostedWindow != null)
            {
                HostedWindow.DragMoveStart -= skin_DragMoveStart;
                HostedWindow.DragMoveStop -= skin_DragMoveStop;
                HostedWindow.ToggleWindowState -= skin_ToggleWindowState;
                HostedWindow.TitleBarMouseMove -= skin_TitleBarMouseMove;
                HostedWindow.DisableWindow();

                var element = (FrameworkElement) HostedWindow;
                ContentGrid.Children.Remove(element);
            }

            skin.CloseRequest += (s, e) => Close();
            skin.DragMoveStart += skin_DragMoveStart;
            skin.DragMoveStop += skin_DragMoveStop;
            skin.ToggleWindowState += skin_ToggleWindowState;
            skin.TitleBarMouseMove += skin_TitleBarMouseMove;

            var appstate = HurricaneSettings.Instance.CurrentState.ApplicationState;
            if (skin != AdvancedWindowSkin && saveinformation)
            {
                appstate.Height = Height;
                appstate.Width = Width;
            }

            HideEqualizer();

            MaxHeight = skin.Configuration.MaxHeight;
            MinHeight = skin.Configuration.MinHeight;
            MaxWidth = skin.Configuration.MaxWidth;
            MinWidth = skin.Configuration.MinWidth;
            ShowTitleBar = skin.Configuration.ShowTitleBar;
            ShowSystemMenuOnRightClick = skin.Configuration.ShowSystemMenuOnRightClick;
            if (skin.Configuration.IsResizable)
            {
                ResizeMode = ResizeMode.CanResize;
                WindowHelper.HideMinimizeAndMaximizeButtons(this);
            }
            else
            {
                ResizeMode = ResizeMode.NoResize;
            }

            if (skin == AdvancedWindowSkin && saveinformation)
            {
                Width = appstate.Width;
                Height = appstate.Height;
            }

            if (skin.Configuration.SupportsCustomBackground)
            {
                SetBackground();
            }
            else
            {
                BackgroundImage.Visibility = Visibility.Collapsed;
                BackgroundImage.Source = null;
                BackgroundMediaElement.Stop();
                BackgroundMediaElement.Source = null;
                BackgroundMediaElement.Visibility = Visibility.Collapsed;
            }
            BackgroundImage.Visibility = skin.Configuration.SupportsCustomBackground ? Visibility.Visible : Visibility.Collapsed;

            if (skin == SmartWindowSkin)
            {
                Width = 300;
                Height = MagicArrow.DockManager.WindowHeight;
                if (MainViewModel.Instance.MusicManager != null)
                    MainViewModel.Instance.MusicManager.DownloadManager.IsOpen = false;
            }

            ShowMinButton = skin.Configuration.ShowWindowControls;
            ShowMaxRestoreButton = skin.Configuration.ShowWindowControls;
            ShowCloseButton = skin.Configuration.ShowWindowControls;

            var newUserControl = (FrameworkElement) skin;
            ContentGrid.Children.Add(newUserControl);
            var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            newUserControl.BeginAnimation(OpacityProperty, animation);
            HostedWindow = skin;
            HostedWindow.EnableWindow();
        }

        #endregion

        #region Events / Closing

        private bool _isHiden;
        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && HurricaneSettings.Instance.Config.MinimizeToTray)
            {
                Hide();
                _isHiden = true;
                NotifyIcon.Visibility = Visibility.Visible;
                if (HurricaneSettings.Instance.Config.ShowNotificationIfMinimizeToTray)
                    NotifyIcon.ShowBalloonTip("Hurricane",
                        Application.Current.Resources["MinimizeToTrayNotification"].ToString(), BalloonIcon.Info);
            }
        }

        private void NotifyIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        public void ShowWindow()
        {
            NotifyIcon.Visibility = Visibility.Hidden;
            Show();
            _isHiden = false;
            WindowState = WindowState.Normal;
            Activate();
        }

        void CSCoreEngine_PlaybackStateChanged(object sender, PlayStateChangedEventArgs e)
        {
            RefreshTaskbarInfo(e.NewPlaybackState);
        }

        public void RefreshTaskbarInfo(PlaybackState playbackState)
        {
            if (!HurricaneSettings.Instance.Config.ShowProgressInTaskbar)
            {
                TaskbarInfo.ProgressState = TaskbarItemProgressState.None;
                return;
            }
            TaskbarInfo.ProgressState = playbackState == PlaybackState.Playing ? TaskbarItemProgressState.Normal : TaskbarItemProgressState.Paused;
        }

        void skin_ToggleWindowState(object sender, EventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        #region Titlebar

        private bool _restoreIfMove;
        void skin_DragMoveStart(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized) _restoreIfMove = true;
            MagicArrow.DockManager.DragStart();
            if (HostedWindow.Configuration.NeedsMovingHelp)
            {
                try
                {
                    DragMove();
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        void skin_DragMoveStop(object sender, EventArgs e)
        {
            _restoreIfMove = false;
            MagicArrow.DockManager.DragStop();
        }

        void skin_TitleBarMouseMove(object sender, MouseEventArgs e)
        {
            if (_restoreIfMove)
            {
                _restoreIfMove = false;

                double percentHorizontal = e.GetPosition(this).X / ActualWidth;
                double targetHorizontal = RestoreBounds.Width * percentHorizontal;

                double percentVertical = e.GetPosition(this).Y / ActualHeight;
                double targetVertical = RestoreBounds.Height * percentVertical;

                WindowState = WindowState.Normal;

                POINT lMousePosition;
                UnsafeNativeMethods.GetCursorPos(out lMousePosition);

                Left = lMousePosition.X - targetHorizontal;
                Top = lMousePosition.Y - targetVertical;

                try
                {
                    DragMove();
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        #endregion

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (MagicArrow.DockManager.CurrentSide == DockingSide.None)
            {
                if (HurricaneSettings.Instance.CurrentState.ApplicationState == null) HurricaneSettings.Instance.CurrentState.ApplicationState = new DockingApplicationState();
                var appstate = HurricaneSettings.Instance.CurrentState.ApplicationState;
                appstate.Height = Height;
                appstate.Width = Width;
                appstate.Left = Left;
                appstate.Top = Top;
                appstate.WindowState = WindowState;
            }
            if (HurricaneSettings.Instance.IsLoaded)
                MagicArrow.DockManager.Save();
            MainViewModel.Instance.Closing();
            MagicArrow.Dispose();
            Application.Current.Shutdown(); //Important if other windows like the track notification are open
        }

        public void BringToFront()
        {
            if (_isHiden)
            {
                ShowWindow();
            }
            else
            {
                MagicArrow.BringToFront();
            }
        }

        #endregion

        #region Messages

        public WindowDialogService WindowDialogService { get; set; }

        EqualizerWindow _equalizerWindow;
        private bool _equalizerIsOpen;

        public void ShowEqualizer()
        {
            if (HostedWindow.Configuration.ShowFullscreenDialogs)
            {
                HurricaneSettings.Instance.CurrentState.EqualizerIsOpen = !HurricaneSettings.Instance.CurrentState.EqualizerIsOpen;
            }
            else
            {
                if (!_equalizerIsOpen)
                {
                    var rect = WindowHelper.GetWindowRectangle(this);
                    _equalizerWindow = new EqualizerWindow(rect, this.ActualWidth);
                    _equalizerWindow.Closed += (s, e) => _equalizerIsOpen = false;
                    _equalizerWindow.BeginCloseAnimation += (s, e) => this.Activate();
                    _equalizerWindow.Show();
                    _equalizerIsOpen = true;
                }
                else
                {
                    _equalizerWindow.Activate();
                }
            }
        }

        private void HideEqualizer()
        {
            if (_equalizerIsOpen) { _equalizerWindow.Close(); _equalizerIsOpen = false; }
        }

        #endregion

        #region Themes

        public async Task MoveOut()
        {
            var fadeanimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            var control = (DependencyObject)HostedWindow;

            Storyboard.SetTarget(fadeanimation, control);

            Storyboard.SetTargetProperty(fadeanimation, new PropertyPath(OpacityProperty));

            var story = new Storyboard();
            story.Children.Add(fadeanimation);
            var handler = new AutoResetEvent(false);
            story.Completed += (s, e) => handler.Set();
            story.Begin(this);
            await Task.Run(() => handler.WaitOne());
            handler.Dispose();
        }

        public async Task ResetAndMoveIn()
        {
            _smartWindowSkin.DisableWindow();
            _advancedWindowSkin.DisableWindow();
            bool isadvancedwindow = HostedWindow != _smartWindowSkin;
            _smartWindowSkin = new WindowSmartView();
            _advancedWindowSkin = new WindowAdvancedView();
            ApplyHostWindow(isadvancedwindow ? _advancedWindowSkin : _smartWindowSkin, false);

            var outanimation = new ThicknessAnimation(new Thickness(-100, 0, 100, 0), new Thickness(0), TimeSpan.FromMilliseconds(350));
            var fadeanimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
            var control = (DependencyObject)HostedWindow;

            Storyboard.SetTarget(outanimation, control);
            Storyboard.SetTarget(fadeanimation, control);

            Storyboard.SetTargetProperty(outanimation, new PropertyPath(MarginProperty));
            Storyboard.SetTargetProperty(fadeanimation, new PropertyPath(OpacityProperty));
            
            var story = new Storyboard();
            story.Children.Add(outanimation);
            story.Children.Add(fadeanimation);
            var handler = new AutoResetEvent(false);
            story.Completed += (s, e) => handler.Set();
            story.Begin(this);
            await Task.Run(() => handler.WaitOne());
            handler.Dispose();
            ResetFlyout();
        }

        public async Task BackgroundChanged()
        {
            await SetBackground();
            var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(1000));
            BackgroundContainer.BeginAnimation(OpacityProperty, animation);
            
        }

        private async Task SetBackground()
        {
            if (HurricaneSettings.Instance.Config.ApplicationDesign.ApplicationBackground == null || !HurricaneSettings.Instance.Config.ApplicationDesign.ApplicationBackground.IsAvailable)
            {
                BackgroundImage.Source = null;
                BackgroundMediaElement.Visibility = Visibility.Collapsed;
                return;
            }

            if (HurricaneSettings.Instance.Config.ApplicationDesign.ApplicationBackground.IsAnimated)
            {
                BackgroundImage.Source = null;
                BackgroundMediaElement.Visibility = Visibility.Visible;
                BackgroundMediaElement.Source =
                    HurricaneSettings.Instance.Config.ApplicationDesign.ApplicationBackground.GetBackground();
                BackgroundMediaElement.Play();
            }
            else
            {
                BackgroundMediaElement.Source = null;
                BackgroundMediaElement.Visibility = Visibility.Collapsed;
                BackgroundImage.Visibility = Visibility.Visible;
                BackgroundImage.Source = await Task.Run(() =>
                {
                    var img =
                        new BitmapImage(
                            HurricaneSettings.Instance.Config.ApplicationDesign.ApplicationBackground.GetBackground());
                    img.Freeze();
                    return img;
                });
            }
        }

        private async void SystemEventsOnPowerModeChanged(object sender, PowerModeChangedEventArgs powerModeChangedEventArgs)
        {
            if (powerModeChangedEventArgs.Mode == PowerModes.Resume)
            {
                BackgroundMediaElement.Source = null; //Else it doesn't get refreshed
                await SetBackground();
            }
        }

        private Flyout _oldFlyout;
        private void ResetFlyout()
        {
            if (_oldFlyout != null) FlyoutControl.Items.Remove(_oldFlyout);
            var newflyout = (Flyout)Resources["DownloadFlyout"];
            FlyoutControl.Items.Add(newflyout);
            _oldFlyout = newflyout;
        }

        #endregion

        private void BackgroundMediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            BackgroundMediaElement.Position = TimeSpan.Zero;
            BackgroundMediaElement.Play();
        }

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }

    public enum DialogMode { Single, First, Last, Following }
}