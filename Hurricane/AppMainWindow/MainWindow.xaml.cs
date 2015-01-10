using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
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
using Hurricane.Settings.Themes;
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

            MagicArrow = new MagicArrow.MagicArrow();
            MagicArrow.Register(this);
            MagicArrow.MoveOut += (s, e) => { HideEqualizer(); HostedWindow.DisableWindow(); };
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
                if (appsettings.ApplicationState.Left < WpfScreen.AllScreensWidth) //To prevent that the window is out of view when the user unplugs a monitor
                {
                    Height = appsettings.ApplicationState.Height;
                    Width = appsettings.ApplicationState.Width;
                    Left = appsettings.ApplicationState.Left;
                    Top = appsettings.ApplicationState.Top;
                    WindowState = appsettings.ApplicationState.WindowState;
                }
                else
                {
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
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
            if (HostedWindow.Configuration.NeedsMovingHelp) DragMove();
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
            if (HurricaneSettings.Instance.IsLoaded)
                MagicArrow.DockManager.Save();
            MainViewModel.Instance.Closing();
            MagicArrow.Dispose();
            Application.Current.Shutdown();
        }

        #endregion

        #region Taskbar

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

        public async Task<bool> ShowMessage(string message, string title, bool cancancel, DialogMode mode)
        {
            if (HostedWindow.Configuration.ShowFullscreenDialogs)
            {
                MessageDialogResult result = await this.ShowMessageAsync(title, message, cancancel ? MessageDialogStyle.AffirmativeAndNegative : MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "OK", NegativeButtonText = Application.Current.Resources["Cancel"].ToString(), AnimateHide = ShowHideAnimation(mode), AnimateShow = ShowShowAnimation(mode), ColorScheme = GetTheme() });
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

        private MetroDialogColorScheme GetTheme()
        {
            return HurricaneSettings.Instance.Config.Theme.BaseTheme == BaseTheme.Light
                ? MetroDialogColorScheme.Theme
                : MetroDialogColorScheme.Accented;
        }

        private bool ShowHideAnimation(DialogMode mode)
        {
            switch (mode)
            {
                case DialogMode.Single:
                    return true;
                case DialogMode.First:
                    return false;
                case DialogMode.Last:
                    return true;
                case DialogMode.Following:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }

        private bool ShowShowAnimation(DialogMode mode)
        {
            switch (mode)
            {
                case DialogMode.Single:
                    return true;
                case DialogMode.First:
                    return true;
                case DialogMode.Last:
                    return false;
                case DialogMode.Following:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }

        public async Task<string> ShowInputDialog(string title, string message, string buttonok, string defaulttext, DialogMode mode)
        {
            if (HostedWindow.Configuration.ShowFullscreenDialogs)
            {
                var dialog = new AdvancedInputDialog(this, new MetroDialogSettings() { AffirmativeButtonText = buttonok, DefaultText = defaulttext, NegativeButtonText = Application.Current.Resources["Cancel"].ToString(), ColorScheme = GetTheme(), AnimateHide = ShowHideAnimation(mode), AnimateShow = ShowShowAnimation(mode) }) { Title = title, Message = message };
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

        EqualizerWindow _equalizerWindow;
        private bool _equalizerIsOpen;

        public async Task ShowEqualizer()
        {
            if (HostedWindow.Configuration.ShowFullscreenDialogs)
            {
                var dialog = new EqualizerDialog(this, new MetroDialogSettings() { ColorScheme = GetTheme() });
                await this.ShowMetroDialogAsync(dialog);
                await dialog.WaitForCloseAsync();
                await this.HideMetroDialogAsync(dialog);
            }
            else
            {
                if (!_equalizerIsOpen)
                {
                    var rect = WindowHelper.GetWindowRectangle(this);
                    _equalizerWindow = new EqualizerWindow(rect, ActualWidth);
                    _equalizerWindow.Closed += (s, e) => _equalizerIsOpen = false;
                    _equalizerWindow.BeginCloseAnimation += (s, e) => Activate();
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
         if(_equalizerIsOpen){   _equalizerWindow.Close(); _equalizerIsOpen = false;}
        }

        public void OpenTrackInformations(Track track)
        {
            TrackInformationWindow trackInformationWindow = new TrackInformationWindow(track) { Owner = this, WindowStartupLocation = this.HostedWindow.Configuration.ShowFullscreenDialogs ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen };
            trackInformationWindow.ShowDialog();
        }

        public void OpenTagEditor(Track track)
        {
            TagEditorWindow tagEditorWindow = new TagEditorWindow(track) { Owner = this, WindowStartupLocation = this.HostedWindow.Configuration.ShowFullscreenDialogs ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen };
            tagEditorWindow.ShowDialog();
        }

        #endregion
    }

    public enum DialogMode { Single, First, Last, Following }
}