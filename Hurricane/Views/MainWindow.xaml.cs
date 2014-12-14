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
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Windows.Interop;
using System.IO;
using System.Runtime.InteropServices;
using Hurricane.Extensions;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace Hurricane
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        public MagicArrow.MagicArrow MagicArrow { get; set; }
        private Hurricane.Resources.Styles.DragDropListView.ServiceProviders.UI.ListViewDragDropManager<Music.Track> dragMgr;

        public MainWindow()
        {
            InitializeComponent();
            System.Windows.Media.MediaTimeline.DesiredFrameRateProperty.OverrideMetadata(typeof(System.Windows.Media.Animation.Timeline), new FrameworkPropertyMetadata(60));

            MagicArrow = new MagicArrow.MagicArrow();
            MagicArrow.Register(this);
            MagicArrow.MoveOut += (s, e) => { ViewModels.MainViewModel.Instance.MoveOut(); SpectrumAnalyzer.RefreshInterval = 2000; };
            MagicArrow.MoveIn += (s, e) => { SpectrumAnalyzer.RefreshInterval = 25; };
            MagicArrow.FilesDropped += (s, e) => { ViewModels.MainViewModel.Instance.DragDropFiles((string[])e.Data.GetData(DataFormats.FileDrop)); };

            this.Closing += MainWindow_Closing;
            this.Loaded += MainWindow_Loaded;
            dragMgr = new Resources.Styles.DragDropListView.ServiceProviders.UI.ListViewDragDropManager<Music.Track>(this.listview);
            dragMgr.ShowDragAdorner = true;

            this.MaxHeight = Utilities.WpfScreen.MaxHeight;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings.HurricaneSettings.Instance.Load();
                MagicArrow.DockManager.InitializeWindow();
                ViewModels.MainViewModel viewmodel = ViewModels.MainViewModel.Instance;
                LoadCustomPreview();
                viewmodel.StartVisualization += CSCoreEngine_StartVisualization;
                viewmodel.TrackChanged += CSCoreEngine_TrackChanged;
                viewmodel.Loaded(this);
                viewmodel.MusicManager.CSCoreEngine.PlayStateChanged += (s, ec) => { thumbnailButtonPlayPause.Icon = viewmodel.MusicManager.CSCoreEngine.CurrentState == CSCore.SoundOut.PlaybackState.Playing ? Utilities.ImageHelper.GetIconFromResource("/Resources/MediaIcons/ThumbButtons/pause.ico") : Utilities.ImageHelper.GetIconFromResource("/Resources/MediaIcons/ThumbButtons/play.ico"); };
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

        #region Thumbnail
        TabbedThumbnail customPreview;
        ThumbnailToolBarButton thumbnailButtonPlayPause;
        protected void LoadCustomPreview()
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            customPreview = new TabbedThumbnail(handle, handle);
            TaskbarManager.Instance.TabbedThumbnail.AddThumbnailPreview(customPreview);
            customPreview.Title = "Hurricane";

            customPreview.SetWindowIcon(Utilities.ImageHelper.GetIconFromResource("Resources/App/icon.ico"));
            customPreview.DisplayFrameAroundBitmap = true;
            customPreview.TabbedThumbnailBitmapRequested += (s, ec) =>
            {
                Music.CSCoreEngine cscore = ViewModels.MainViewModel.Instance.MusicManager.CSCoreEngine;
                if (cscore.CurrentTrack != null && cscore.CurrentTrack.Image != null)
                {
                    var img = cscore.CurrentTrack.Image;
                    int maxwidth = 220;
                    int maxheight = 141;

                    System.Drawing.Size newsize = Utilities.ImageHelper.GetMinimumSize(img.Size, maxwidth, maxheight);
                    System.Drawing.Bitmap bit = Utilities.ImageHelper.ResizeImage(new System.Drawing.Bitmap(img), newsize);
                    System.Drawing.Bitmap bittodisplay = bit.Clone(new System.Drawing.RectangleF(0, 0, maxwidth, maxheight), img.PixelFormat);
                    bit.Dispose();

                    customPreview.SetImage(bittodisplay);
                }
                else
                { customPreview.SetImage(new System.Drawing.Bitmap(Application.GetResourceStream(new Uri("pack://application:,,,/Hurricane;component/Resources/MediaIcons/Thumbnail/StandardThumbnail.png")).Stream)); }
            };

            ThumbnailToolBarButton thumbnailButtonBack = new ThumbnailToolBarButton(Utilities.ImageHelper.GetIconFromResource("/Resources/MediaIcons/ThumbButtons/backward.ico"), Application.Current.FindResource("previoustrack").ToString());
            thumbnailButtonBack.Click += (s, e) => { ViewModels.MainViewModel.Instance.MusicManager.GoBackward(); };

            thumbnailButtonPlayPause = new ThumbnailToolBarButton(Utilities.ImageHelper.GetIconFromResource("/Resources/MediaIcons/ThumbButtons/play.ico"), "Play / Pause");
            thumbnailButtonPlayPause.Click += (s, e) => { ViewModels.MainViewModel.Instance.MusicManager.CSCoreEngine.TogglePlayPause(); };

            ThumbnailToolBarButton thumbnailButtonNext = new ThumbnailToolBarButton(Utilities.ImageHelper.GetIconFromResource("/Resources/MediaIcons/ThumbButtons/forward.ico"), Application.Current.FindResource("nexttrack").ToString());
            thumbnailButtonNext.Click += (s, e) => { ViewModels.MainViewModel.Instance.MusicManager.GoForward(); };

            TaskbarManager.Instance.ThumbnailToolBars.AddButtons(handle, thumbnailButtonBack, thumbnailButtonPlayPause, thumbnailButtonNext);
        }

        void CSCoreEngine_TrackChanged(object sender, Music.TrackChangedEventArgs e)
        {
            if (e.NewTrack != null)
            {
                customPreview.Title = e.NewTrack.ToString();
                customPreview.InvalidatePreview();
            }
        }
        #endregion

        #region Controllogic
        void CSCoreEngine_StartVisualization(object sender, EventArgs e)
        {
            SpectrumAnalyzer.RegisterSoundPlayer(ViewModels.MainViewModel.Instance.MusicManager.CSCoreEngine);
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Settings.HurricaneSettings.Instance.Loaded)
                MagicArrow.DockManager.Save();
            ViewModels.MainViewModel.Instance.Closing();
            MagicArrow.Dispose();
            App.Current.Shutdown();
        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Effects == DragDropEffects.None)
                return;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                ViewModels.MainViewModel.Instance.DragDropFiles((string[])e.Data.GetData(DataFormats.FileDrop));
            }
        }

        private void ListView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move; //Always move because if we would check if it's a file or not, the drag & drop function for the items wouldn't work
        }
        
        #endregion

        #region Windowlogic
        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MagicArrow.DockManager.DragStart();
            DragMove();
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PART_TITLEBAR_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MagicArrow.DockManager.DragStop();
        }

        private void buttonplus_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = ((Button)sender).ContextMenu;
            menu.PlacementTarget = (UIElement)sender;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            menu.IsOpen = true;
        }

        private void listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listview.ScrollIntoView(listview.SelectedItem);
        }
        #endregion

        #region Animations
        private void GridCurrentTrack_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid == null) return;
            CurrentTrackAnimation(grid, txtCurrentTrack, polyplay, true);
        }

        private void GridCurrentTrack_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid == null) return;
            CurrentTrackAnimation(grid, txtCurrentTrack, polyplay, false);
        }

        private void CurrentTrackAnimation(Grid grid, TextBlock txt, Polygon poly, bool InAnimate)
        {
            Storyboard story = new Storyboard();
            ColorAnimation coloranimation = new ColorAnimation(InAnimate ? (Color)Application.Current.FindResource("AccentColor") : Colors.Transparent, TimeSpan.FromMilliseconds(500));
            Storyboard.SetTarget(coloranimation, grid);
            Storyboard.SetTargetProperty(coloranimation, new PropertyPath("Background.Color"));

            ColorAnimation coloranimation2 = new ColorAnimation(InAnimate ? ((SolidColorBrush)Application.Current.FindResource("DarkColorBrush")).Color : Colors.Black, TimeSpan.FromMilliseconds(250));
            Storyboard.SetTarget(coloranimation2, txtCurrentTrack);
            Storyboard.SetTargetProperty(coloranimation2, new PropertyPath("Foreground.Color"));

            ThicknessAnimation thicknessanimation = new ThicknessAnimation(InAnimate ? new Thickness(3, 2, -3, 0) : new Thickness(0,2,0,0), TimeSpan.FromMilliseconds(250));
            Storyboard.SetTarget(thicknessanimation, poly);
            Storyboard.SetTargetProperty(thicknessanimation, new PropertyPath(Polygon.MarginProperty));

            //story.Children.Add(coloranimation);
            story.Children.Add(coloranimation2);
            story.Children.Add(thicknessanimation);
            story.Begin(this);
        }
        #endregion
    }
}
