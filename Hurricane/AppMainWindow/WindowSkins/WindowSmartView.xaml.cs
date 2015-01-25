using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Hurricane.Music;
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
                IsResizable = false
            };
        }

        #region CurrentTrackAnimation
        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            CurrentTrackAnimation(txtCurrentTrack, polyplay, true);
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            CurrentTrackAnimation(txtCurrentTrack, polyplay, false);
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
            listview.ScrollIntoView(listview.SelectedItem);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (CloseRequest != null) CloseRequest(this, EventArgs.Empty);
        }

        private void titlebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DragMoveStart != null) DragMoveStart(this, EventArgs.Empty);
        }

        private void titlebar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
            SpectrumAnalyzer.RefreshInterval = 20;
        }

        public void DisableWindow()
        {
            SpectrumAnalyzer.RefreshInterval = int.MaxValue;
        }

        public void RegisterSoundPlayer(CSCoreEngine engine)
        {
            this.SpectrumAnalyzer.RegisterSoundPlayer(engine);
        }

        protected WindowSkinConfiguration configuration;
        public WindowSkinConfiguration Configuration
        {
            get
            {
                return configuration;
            }
            set
            {
                configuration = value;
            }
        }

        public void MusicManagerEnabled(object manager)
        {
            
        }
    }
}
