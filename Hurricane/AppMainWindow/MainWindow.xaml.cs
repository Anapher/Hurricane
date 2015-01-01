using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shell;
using System.Windows.Threading;
using CSCore.SoundOut;
using Hurricane.AppMainWindow.MahAppsExtensions.Dialogs;
using Hurricane.AppMainWindow.Messages;
using Hurricane.AppMainWindow.WindowSkins;
using Hurricane.MagicArrow.DockManager;
using Hurricane.Music;
using Hurricane.Music.MusicDatabase.EventArgs;
using Hurricane.Settings;
using Hurricane.Utilities;
using Hurricane.ViewModels;
using Hurricane.Views;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using InputDialog = Hurricane.Views.InputDialog;

namespace Hurricane
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MagicArrow.MagicArrow MagicArrow { get; set; }
        public bool IsInSmartMode { get; set; }
        public IWindowSkin HostedWindow { get; set; }

        protected IWindowSkin smartwindowskin;
        public IWindowSkin SmartWindowSkin { get { return smartwindowskin ?? (smartwindowskin = new WindowSmartView()); } }

        protected IWindowSkin advancedwindowskin;
        public IWindowSkin AdvancedWindowSkin { get { return advancedwindowskin ?? (advancedwindowskin = new WindowAdvancedView()); } }


        #region Constructor & Load

        public MainWindow()
        {
            InitializeComponent();
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata(60));

            MagicArrow = new MagicArrow.MagicArrow();
            MagicArrow.Register(this);
            MagicArrow.MoveOut += (s, e) => { MainViewModel.Instance.MoveOut(); HostedWindow.DisableWindow(); };
            MagicArrow.MoveIn += (s, e) => { HostedWindow.EnableWindow(); };
            MagicArrow.FilesDropped += (s, e) => { MainViewModel.Instance.DragDropFiles((string[])e.Data.GetData(DataFormats.FileDrop)); };

            Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;

            MagicArrow.DockManager.Docked += (s, e) => { ApplyHostWindow(SmartWindowSkin); };
            MagicArrow.DockManager.Undocked += (s, e) =>
            {
                if (HurricaneSettings.Instance.Config.EnableAdvancedView) ApplyHostWindow(AdvancedWindowSkin);
            };

            var appsettings = HurricaneSettings.Instance.Config;
            if (appsettings.ApplicationState == null)
            {
                appsettings.ApplicationState = new DockingApplicationState();
                appsettings.ApplicationState.CurrentSide = DockingSide.None;
                appsettings.ApplicationState.Height = 600;
                appsettings.ApplicationState.Width = 1000;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                appsettings.ApplicationState.Left = Left;
                appsettings.ApplicationState.Top = Top;
            }

            if (appsettings.ApplicationState.CurrentSide == DockingSide.None)
            {
                Height = appsettings.ApplicationState.Height;
                Width = appsettings.ApplicationState.Width;
                Left = appsettings.ApplicationState.Left;
                Top = appsettings.ApplicationState.Top;
                WindowState = appsettings.ApplicationState.WindowState;
            }

            MagicArrow.DockManager.CurrentSide = appsettings.ApplicationState.CurrentSide;
            InitializeMessages();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MagicArrow.DockManager.ApplyCurrentSide();
                if (MagicArrow.DockManager.CurrentSide == DockingSide.None && HurricaneSettings.Instance.Config.EnableAdvancedView)
                {
                    ApplyHostWindow(AdvancedWindowSkin, false);
                }
                else
                {
                    ApplyHostWindow(SmartWindowSkin, false);
                    Height = WpfScreen.GetScreenFrom(new Point(Left, 0)).WorkingArea.Height;
                }

                MainViewModel viewmodel = MainViewModel.Instance;
                viewmodel.StartVisualization += CSCoreEngine_StartVisualization;
                viewmodel.PositionChanged += viewmodel_PositionChanged;
                viewmodel.Loaded(this);
                viewmodel.MusicManager.CSCoreEngine.PlaybackStateChanged += CSCoreEngine_PlaybackStateChanged;

                AdvancedWindowSkin.MusicManagerEnabled(viewmodel.MusicManager);
                SmartWindowSkin.MusicManagerEnabled(viewmodel.MusicManager);
            }
            catch (Exception ex)
            {
#if (!DEBUG)
                Views.ReportExceptionWindow window = new Views.ReportExceptionWindow(ex) { Owner = this };
                window.ShowDialog();
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
                HostedWindow.DisableWindow();
            }

            skin.CloseRequest += (s, e) => Close();
            skin.DragMoveStart += skin_DragMoveStart;
            skin.DragMoveStop += skin_DragMoveStop;
            skin.ToggleWindowState += skin_ToggleWindowState;

            var appstate = HurricaneSettings.Instance.Config.ApplicationState;
            if (skin != AdvancedWindowSkin && saveinformation)
            {
                appstate.Height = Height;
                appstate.Width = Width;
            }

            MainViewModel.Instance.CloseEqualizer();

            MaxHeight = skin.Configuration.MaxHeight;
            MinHeight = skin.Configuration.MinHeight;
            MaxWidth = skin.Configuration.MaxWidth;
            MinWidth = skin.Configuration.MinWidth;
            ShowTitleBar = skin.Configuration.ShowTitleBar;
            ShowSystemMenuOnRightClick = skin.Configuration.ShowSystemMenuOnRightClick;

            if (skin == AdvancedWindowSkin && saveinformation)
            {
                Width = appstate.Width;
                Height = appstate.Height;
            }

            if (skin == SmartWindowSkin) { Width = 300; Height = MagicArrow.DockManager.WindowHeight; }

            ShowMinButton = skin.Configuration.ShowWindowControls;
            ShowMaxRestoreButton = skin.Configuration.ShowWindowControls;
            ShowCloseButton = skin.Configuration.ShowWindowControls;

            Content = skin;
            HostedWindow = skin;
            if (MainViewModel.Instance.MusicManager != null) skin.RegisterSoundPlayer(MainViewModel.Instance.MusicManager.CSCoreEngine);
            HostedWindow.EnableWindow();
        }

        #endregion

        #region Events / Closing

        void skin_ToggleWindowState(object sender, EventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        protected override void TitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            MagicArrow.DockManager.DragStart();
            base.TitleBarMouseDown(sender, e);
        }

        protected override void TitleBarMouseUp(object sender, MouseButtonEventArgs e)
        {
            MagicArrow.DockManager.DragStop();
            base.TitleBarMouseUp(sender, e);
        }

        void skin_DragMoveStart(object sender, EventArgs e)
        {
            MagicArrow.DockManager.DragStart();
            if (HostedWindow.Configuration.NeedMovingHelp) DragMove();
        }

        void skin_DragMoveStop(object sender, EventArgs e)
        {
            MagicArrow.DockManager.DragStop();
        }

        void CSCoreEngine_StartVisualization(object sender, EventArgs e)
        {
            HostedWindow.RegisterSoundPlayer(MainViewModel.Instance.MusicManager.CSCoreEngine);
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (MagicArrow.DockManager.CurrentSide == DockingSide.None)
            {
                if (HurricaneSettings.Instance.Config.ApplicationState == null) HurricaneSettings.Instance.Config.ApplicationState = new DockingApplicationState();
                var appstate = HurricaneSettings.Instance.Config.ApplicationState;
                appstate.Height = Height;
                appstate.Width = Width;
                appstate.Left = Left;
                appstate.Top = Top;
                appstate.WindowState = WindowState;
            }
            if (HurricaneSettings.Instance.Loaded)
                MagicArrow.DockManager.Save();
            MainViewModel.Instance.Closing();
            MagicArrow.Dispose();
            Application.Current.Shutdown();
        }

        #endregion

        #region Taskbar

        void viewmodel_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            double progress = e.NewPosition / (double)e.TrackLength;
            if (taskbarinfo.ProgressValue != progress) taskbarinfo.ProgressValue = progress;
        }

        void CSCoreEngine_PlaybackStateChanged(object sender, PlayStateChangedEventArgs e)
        {
            taskbarinfo.ProgressState = e.NewPlaybackState == PlaybackState.Playing ? TaskbarItemProgressState.Normal : TaskbarItemProgressState.Paused;
        }

        #endregion

        #region Messages

        public MessageManager Messages { get; set; }

        protected void InitializeMessages()
        {
            Messages = new MessageManager { ProgressDialogStart = Messages_ProgressDialogStart };
        }

        public async Task<bool> ShowMessage(string message, string title, bool cancancel)
        {
            if (HostedWindow.Configuration.ShowFullscreenDialogs)
            {
                MessageDialogResult result = await this.ShowMessageAsync(title, message, cancancel ? MessageDialogStyle.AffirmativeAndNegative : MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "OK", NegativeButtonText = Application.Current.FindResource("Cancel").ToString(), AnimateHide = false });
                return result == MessageDialogResult.Affirmative;
            }
            else
            {
                MessageWindow messageWindow = new MessageWindow(message, title, cancancel) { Owner = this };
                bool result = false;
                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => result = messageWindow.ShowDialog() == true));
                return result;
            }
        }

        public async Task<string> ShowInputDialog(string title, string message, string buttonok, string defaulttext)
        {
            if (HostedWindow.Configuration.ShowFullscreenDialogs)
            {
                var dialog = new AdvancedInputDialog(this, new MetroDialogSettings() { AffirmativeButtonText = buttonok, DefaultText = defaulttext, NegativeButtonText = Application.Current.FindResource("Cancel").ToString() }) { Title = title, Message = message };
                await this.ShowMetroDialogAsync(dialog);
                string result = await dialog.WaitForButtonPressAsync();
                await dialog._WaitForCloseAsync();
                var asd = this.HideMetroDialogAsync(dialog);
                return result;
            }
            else
            {
                InputDialog inputdialog = new InputDialog(title, message, buttonok, defaulttext) { Owner = this };
                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => inputdialog.ShowDialog()));
                return inputdialog.ResultText;
            }
        }

        public async Task ShowTrackInformation(Track track)
        {
            if (HostedWindow.Configuration.ShowFullscreenDialogs)
            {
                var dialog = new TrackInformationDialog(this, track, null);
                await this.ShowMetroDialogAsync(dialog);
                await dialog.WaitForCloseAsync();
                await this.HideMetroDialogAsync(dialog);
            }
            else
            {
                TrackInformationWindow trackInformationWindow = new TrackInformationWindow(track) { Owner = this };
                trackInformationWindow.ShowDialog();
            }
        }

        async void Messages_ProgressDialogStart(ProgressDialogStartEventArgs e)
        {
            if (HostedWindow.Configuration.ShowFullscreenDialogs)
            {
                var progresscontroller = await this.ShowProgressAsync(e.Title, string.Empty);
                progresscontroller.SetIndeterminate();
                e.Instance.MessageChanged = ev =>
                    progresscontroller.SetMessage(ev);
                e.Instance.TitleChanged = ev =>
                    progresscontroller.SetTitle(ev);
                e.Instance.ProgressChanged = ev =>
                    progresscontroller.SetProgress(ev);
                e.Instance.CloseRequest = () =>
                 progresscontroller.CloseAsync();
                if (e.Instance.IsClosed) await progresscontroller.CloseAsync();
            }
            else
            {
                ProgressWindow progressWindow = new ProgressWindow(e.Title, e.IsIndeterminate) { Owner = this };
                e.Instance.MessageChanged = ev => progressWindow.SetText(ev);
                e.Instance.TitleChanged = ev => progressWindow.SetTitle(ev);
                e.Instance.ProgressChanged = ev => progressWindow.SetProgress(ev);
                e.Instance.CloseRequest = () => { progressWindow.Close(); return null; };
                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => progressWindow.ShowDialog()));
            }
        }

        #endregion

    }
}
