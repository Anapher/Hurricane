using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Hurricane.PluginAPI.AudioVisualisation;
using Hurricane.Utilities;
using Hurricane.ViewModels;

namespace Hurricane.AppMainWindow.WindowSkins
{
    /// <summary>
    /// Interaction logic for WindowSmartView.xaml
    /// </summary>
    public partial class WindowSmartView : IWindowSkin
    {
        public WindowSmartView()
        {
            InitializeComponent();
            Configuration = new WindowSkinConfiguration()
            {
                MaxHeight = WpfScreen.MaxHeight,
                MaxWidth = 300,
                MinHeight = 400,
                MinWidth = 300,
                ShowSystemMenuOnRightClick = false,
                ShowTitleBar = false,
                ShowWindowControls = false,
                NeedsMovingHelp = true,
                ShowFullscreenDialogs = false,
                IsResizable = false,
                SupportsCustomBackground = false,
                SupportsMinimizingToTray = false
            };
        }

        #region CurrentTrackAnimation
        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            var textBlock = TitleTransitioningControl.Content as TextBlock;
            if (textBlock == null) return;
            CurrentTrackAnimation(textBlock, PlayPolygon, true);
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            var textBlock = TitleTransitioningControl.Content as TextBlock;
            if (textBlock == null) return;
            CurrentTrackAnimation(textBlock, PlayPolygon, false);
        }

        private void CurrentTrackAnimation(TextBlock txt, Polygon poly, bool inAnimate)
        {
            Storyboard story = new Storyboard();

            ColorAnimation coloranimation2 = new ColorAnimation(inAnimate ? ((SolidColorBrush)Application.Current.Resources["AccentColorBrush"]).Color : (Color)Application.Current.Resources["BlackColor"], TimeSpan.FromMilliseconds(250));
            Storyboard.SetTarget(coloranimation2, txt);
            Storyboard.SetTargetProperty(coloranimation2, new PropertyPath("Foreground.Color"));

            ThicknessAnimation thicknessanimation = new ThicknessAnimation(inAnimate ? new Thickness(3, 2, -3, 0) : new Thickness(0, 2, 0, 0), TimeSpan.FromMilliseconds(250));
            Storyboard.SetTarget(thicknessanimation, poly);
            Storyboard.SetTargetProperty(thicknessanimation, new PropertyPath(MarginProperty));

            story.Children.Add(coloranimation2);
            story.Children.Add(thicknessanimation);
            story.Begin(this);
        }
        #endregion


        private void listview_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move; //Always move because if we would check if it's a file or not, the drag & drop function for the items wouldn't work
        }

        private void listview_Drop(object sender, DragEventArgs e)
        {
            if (e.Effects == DragDropEffects.None)
                return;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                MainViewModel.Instance.DragDropFiles((string[])e.Data.GetData(DataFormats.FileDrop));
            }
        }

        private void listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TracksListView.ScrollIntoView(TracksListView.SelectedItem);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CloseRequest != null) CloseRequest(this, EventArgs.Empty);
        }

        private void Titlebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DragMoveStart != null) DragMoveStart(this, EventArgs.Empty);
        }

        private void Titlebar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DragMoveStop != null) DragMoveStop(this, EventArgs.Empty);
        }

        public event EventHandler DragMoveStart;

        public event EventHandler DragMoveStop;

        public event EventHandler CloseRequest;

        public event EventHandler ToggleWindowState
        {
            add { }
            remove { }
        }

        public event EventHandler<MouseEventArgs> TitleBarMouseMove
        {
            add { }
            remove { }
        }

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
    }
}
