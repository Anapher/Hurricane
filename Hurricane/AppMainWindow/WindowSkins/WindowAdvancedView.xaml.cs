using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AudioVisualisation;
using Hurricane.Music;
using Hurricane.ViewModels;

namespace Hurricane.AppMainWindow.WindowSkins
{
    /// <summary>
    /// Interaction logic for WindowAdvancedView.xaml
    /// </summary>
    public partial class WindowAdvancedView : IWindowSkin
    {
        public WindowAdvancedView()
        {
            InitializeComponent();

            Configuration = new WindowSkinConfiguration()
            {
                MaxHeight = double.PositiveInfinity,
                MaxWidth = double.PositiveInfinity,
                MinHeight = 500,
                MinWidth = 850,
                ShowSystemMenuOnRightClick = true,
                ShowTitleBar = false,
                ShowWindowControls = true,
                NeedsMovingHelp = true,
                ShowFullscreenDialogs = true,
                IsResizable = true,
                SupportsCustomBackground = true,
                SupportsMinimizingToTray = true
            };

            SettingsViewModel.Instance.Load();
        }

        public event EventHandler DragMoveStart;

        public event EventHandler DragMoveStop;

        public event EventHandler CloseRequest
        {
            add { }
            remove { }
        }

        public event EventHandler ToggleWindowState;
        public event EventHandler<MouseEventArgs> TitleBarMouseMove;

        public void EnableWindow()
        {
            var visulisation = AudioVisualisationContentControl.Tag as IAudioVisualisation;
            if (visulisation != null) visulisation.Enable();
        }

        public void DisableWindow()
        {
            var visulisation = AudioVisualisationContentControl.Tag as IAudioVisualisation;
            if (visulisation != null) visulisation.Disable();
        }

        public WindowSkinConfiguration Configuration { get; set; }

        #region Titlebar


        private void Titlebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (ToggleWindowState != null) ToggleWindowState(this, EventArgs.Empty);
                return;
            }

            if (DragMoveStart != null) DragMoveStart(this, EventArgs.Empty);
        }

        private void Titlebar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DragMoveStop != null) DragMoveStop(this, EventArgs.Empty);
        }

        private void Titlebar_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (TitleBarMouseMove != null) TitleBarMouseMove(this, e);
        }

        #endregion

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var trackListView = (ListView)sender;
            trackListView.ScrollIntoView(trackListView.SelectedItem);
        }

        private void ListView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Effects == DragDropEffects.None)
                return;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                MainViewModel.Instance.DragDropFiles((string[])e.Data.GetData(DataFormats.FileDrop));
            }
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MainViewModel.Instance.MusicManager.Commands.PlaySelectedTrack.Execute(null);
        }
    }
}
