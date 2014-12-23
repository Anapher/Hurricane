using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.IO;
using System.Runtime.InteropServices;
using Hurricane.Extensions;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using Hurricane.AppMainWindow.WindowSkins;
using Hurricane.AppMainWindow;

namespace Hurricane
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        public MagicArrow.MagicArrow MagicArrow { get; set; }
        public bool IsInSmartMode { get; set; }
        public IWindowSkin HostedWindow { get; set; }

        protected IWindowSkin smartwindowskin;
        public IWindowSkin SmartWindowSkin { get { if (smartwindowskin == null) smartwindowskin = new WindowSmartView(); return smartwindowskin; } }

        protected IWindowSkin advancedwindowskin;
        public IWindowSkin AdvancedWindowSkin { get { if (advancedwindowskin == null) advancedwindowskin = new WindowAdvancedView(); return advancedwindowskin; } }

        public MainWindow()
        {
            InitializeComponent();
            System.Windows.Media.MediaTimeline.DesiredFrameRateProperty.OverrideMetadata(typeof(System.Windows.Media.Animation.Timeline), new FrameworkPropertyMetadata(60));

            MagicArrow = new MagicArrow.MagicArrow();
            MagicArrow.Register(this);
            MagicArrow.MoveOut += (s, e) => { ViewModels.MainViewModel.Instance.MoveOut(); HostedWindow.DisableWindow(); };
            MagicArrow.MoveIn += (s, e) => { HostedWindow.EnableWindow(); };
            MagicArrow.FilesDropped += (s, e) => { ViewModels.MainViewModel.Instance.DragDropFiles((string[])e.Data.GetData(DataFormats.FileDrop)); };

            this.Closing += MainWindow_Closing;
            this.Loaded += MainWindow_Loaded;

            MagicArrow.DockManager.Docked += (s, e) => { ApplyHostWindow(SmartWindowSkin); };
            MagicArrow.DockManager.Undocked += (s, e) => { 
                if(Hurricane.Settings.HurricaneSettings.Instance.Config.EnableAdvancedView) ApplyHostWindow(AdvancedWindowSkin);
            };

            var appsettings = Hurricane.Settings.HurricaneSettings.Instance.Config;
            if (appsettings.ApplicationState == null)
            {
                appsettings.ApplicationState = new MagicArrow.DockManager.DockingApplicationState();
                appsettings.ApplicationState.CurrentSide = Hurricane.MagicArrow.DockManager.DockingSide.None;
                appsettings.ApplicationState.Height = 600;
                appsettings.ApplicationState.Width = 1000;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                appsettings.ApplicationState.Left = this.Left;
                appsettings.ApplicationState.Top = this.Top;
            }

            if (appsettings.ApplicationState.CurrentSide == Hurricane.MagicArrow.DockManager.DockingSide.None)
            {
                this.Height = appsettings.ApplicationState.Height;
                this.Width = appsettings.ApplicationState.Width;
                this.Left = appsettings.ApplicationState.Left;
                this.Top = appsettings.ApplicationState.Top;
            }

            MagicArrow.DockManager.CurrentSide = appsettings.ApplicationState.CurrentSide;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MagicArrow.DockManager.ApplyCurrentSide();
                if (MagicArrow.DockManager.CurrentSide == Hurricane.MagicArrow.DockManager.DockingSide.None && Hurricane.Settings.HurricaneSettings.Instance.Config.EnableAdvancedView)
                {
                    ApplyHostWindow(AdvancedWindowSkin, false);
                }
                else
                {
                    ApplyHostWindow(SmartWindowSkin, false);
                    this.Height = Utilities.WpfScreen.GetScreenFrom(new System.Windows.Point(this.Left, 0)).WorkingArea.Height;
                }
                System.Diagnostics.Debug.Print("ThisHeight: {0}", this.Height.ToString());
                ViewModels.MainViewModel viewmodel = ViewModels.MainViewModel.Instance;
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

        protected void ApplyHostWindow(IWindowSkin skin, bool saveinformations = true)
        {
            System.Diagnostics.Debug.Print("hallo: "+ (skin == AdvancedWindowSkin).ToString());
            if (skin == HostedWindow) return;
            if (HostedWindow != null)
            {
                HostedWindow.DragMoveStart -= skin_DragMoveStart;
                HostedWindow.DragMoveStop -= skin_DragMoveStop;
                HostedWindow.ToggleWindowState -= skin_ToggleWindowState;
                HostedWindow.DisableWindow();
            }

            skin.CloseRequest += (s, e) => this.Close();
            skin.DragMoveStart += skin_DragMoveStart;
            skin.DragMoveStop += skin_DragMoveStop;
            skin.ToggleWindowState += skin_ToggleWindowState;

            var appstate = Hurricane.Settings.HurricaneSettings.Instance.Config.ApplicationState;
            if (skin != AdvancedWindowSkin && saveinformations)
            {
                appstate.Height = this.Height;
                appstate.Width = this.Width;
                System.Diagnostics.Debug.Print("Saved: {0}, {1}", this.ActualHeight, this.Width);
            }

            this.MaxHeight = skin.Configuration.MaxHeight;
            this.MinHeight = skin.Configuration.MinHeight;
            this.MaxWidth = skin.Configuration.MaxWidth;
            this.MinWidth = skin.Configuration.MinWidth;
            this.ShowTitleBar = skin.Configuration.ShowTitleBar;
            this.ShowSystemMenuOnRightClick = skin.Configuration.ShowSystemMenuOnRightClick;

            if (skin == AdvancedWindowSkin && saveinformations)
            {
                System.Diagnostics.Debug.Print("Loaded: {0}, {1} : {2}, {3}", appstate.Height.ToString(), appstate.Width, this.MaxHeight, this.MaxWidth);
                this.Width = appstate.Width;
                this.Height = appstate.Height;
                if (this.Width != appstate.Width) System.Diagnostics.Debug.Print("fuck");
            }

            if (skin == SmartWindowSkin) { this.Width = 300; this.Height = MagicArrow.DockManager.WindowHeight; }

            this.ShowMinButton = skin.Configuration.ShowWindowControls;
            this.ShowMaxRestoreButton = skin.Configuration.ShowWindowControls;
            this.ShowCloseButton = skin.Configuration.ShowWindowControls;

            this.Content = skin;
            this.HostedWindow = skin;
            if (ViewModels.MainViewModel.Instance.MusicManager != null) skin.RegisterSoundPlayer(ViewModels.MainViewModel.Instance.MusicManager.CSCoreEngine);
            HostedWindow.EnableWindow();
        }

        void skin_ToggleWindowState(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Normal) { this.WindowState = WindowState.Maximized; } else { this.WindowState = System.Windows.WindowState.Normal; }
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

        #region Taskbar

        void viewmodel_PositionChanged(object sender, Music.PositionChangedEventArgs e)
        {
            double progress = e.NewPosition / (double)e.TrackLength;
            if (taskbarinfo.ProgressValue != progress) this.taskbarinfo.ProgressValue = progress;
        }

        void CSCoreEngine_PlaybackStateChanged(object sender, Music.PlayStateChangedEventArgs e)
        {
            if (e.NewPlaybackState == CSCore.SoundOut.PlaybackState.Playing)
            {
                this.taskbarinfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
            }
            else
            {
                this.taskbarinfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
            }
        }
        #endregion

        #region Controllogic
        void CSCoreEngine_StartVisualization(object sender, EventArgs e)
        {
            HostedWindow.RegisterSoundPlayer(ViewModels.MainViewModel.Instance.MusicManager.CSCoreEngine);
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MagicArrow.DockManager.CurrentSide == Hurricane.MagicArrow.DockManager.DockingSide.None)
            {
                if (Hurricane.Settings.HurricaneSettings.Instance.Config.ApplicationState == null) Hurricane.Settings.HurricaneSettings.Instance.Config.ApplicationState = new Hurricane.MagicArrow.DockManager.DockingApplicationState();
                var appstate = Hurricane.Settings.HurricaneSettings.Instance.Config.ApplicationState;
                appstate.Height = this.Height;
                appstate.Width = this.Width;
                appstate.Left = this.Left;
                appstate.Top = this.Top;
            }
            if (Settings.HurricaneSettings.Instance.Loaded)
                MagicArrow.DockManager.Save();
            ViewModels.MainViewModel.Instance.Closing();
            MagicArrow.Dispose();
            //CustomTaskbarManager.Dispose();
            App.Current.Shutdown();
        }
        #endregion
    }
}
