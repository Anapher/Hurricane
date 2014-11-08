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
using System.Drawing;
using System.IO;

namespace Hurricane
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window  
    {

        private MagicArrow.MagicArrow MagicArrow;
        private Hurricane.Resources.Styles.DragDropListView.ServiceProviders.UI.ListViewDragDropManager<Music.Track> dragMgr;

        public MainWindow()
        {
            InitializeComponent();
            this.Left = 0;
            this.Top = 0;
            this.Height = System.Windows.SystemParameters.WorkArea.Height;

            MagicArrow = new MagicArrow.MagicArrow();
            MagicArrow.Register(this);
            MagicArrow.MoveOut += (s, e) => { ViewModels.MainViewModel.Instance.MoveOut(); };
            
            this.Closing += MainWindow_Closing;
            this.Loaded += MainWindow_Loaded;
            dragMgr = new Resources.Styles.DragDropListView.ServiceProviders.UI.ListViewDragDropManager<Music.Track>(this.listview);
            dragMgr.ShowDragAdorner = true;
        }

        TabbedThumbnail customPreview;
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModels.MainViewModel viewmodel = ViewModels.MainViewModel.Instance;
                LoadCustomPreview();
                viewmodel.StartVisualization += CSCoreEngine_StartVisualization;
                viewmodel.TrackChanged += CSCoreEngine_TrackChanged;
                viewmodel.Loaded(this);
                viewmodel.MusicEngine.CSCoreEngine.PlayStateChanged += (s, ec) => { thumbnailButtonPlayPause.Icon = viewmodel.MusicEngine.CSCoreEngine.CurrentState == CSCore.SoundOut.PlaybackState.Playing ? Utilities.ImageHelper.GetIconFromResource("/Resources/MediaIcons/ThumbButtons/pause.ico") : Utilities.ImageHelper.GetIconFromResource("/Resources/MediaIcons/ThumbButtons/play.ico"); };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

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
                Music.CSCore cscore = ViewModels.MainViewModel.Instance.MusicEngine.CSCoreEngine;
                if (cscore.CurrentTrack != null && cscore.CurrentTrack.Image != null)
                {
                    var img = cscore.CurrentTrack.Image;
                    int maxwidth = 220;
                    int maxheight = 141;

                    System.Drawing.Size newsize = Utilities.ImageHelper.GetMinimumSize(img.Size, maxwidth, maxheight);
                    Bitmap bit = Utilities.ImageHelper.ResizeImage(new Bitmap(img), newsize);
                    Bitmap bittodisplay = bit.Clone(new RectangleF(0, 0, maxwidth, maxheight), img.PixelFormat);
                    bit.Dispose();

                    customPreview.SetImage(bittodisplay);
                }
                else
                { customPreview.SetImage(new Bitmap(Application.GetResourceStream(new Uri("pack://application:,,,/Hurricane;component/Resources/MediaIcons/Thumbnail/StandardThumbnail.png")).Stream)); }
            };

            ThumbnailToolBarButton thumbnailButtonBack = new ThumbnailToolBarButton(Utilities.ImageHelper.GetIconFromResource("/Resources/MediaIcons/ThumbButtons/backward.ico"), Application.Current.FindResource("previoustrack").ToString());
            thumbnailButtonBack.Click += (s, e) => { ViewModels.MainViewModel.Instance.MusicEngine.GoBackward(); };

            thumbnailButtonPlayPause = new ThumbnailToolBarButton(Utilities.ImageHelper.GetIconFromResource("/Resources/MediaIcons/ThumbButtons/play.ico"), "Play / Pause");
            thumbnailButtonPlayPause.Click += (s, e) => { ViewModels.MainViewModel.Instance.MusicEngine.CSCoreEngine.TogglePlayPause(); };

            ThumbnailToolBarButton thumbnailButtonNext = new ThumbnailToolBarButton(Utilities.ImageHelper.GetIconFromResource("/Resources/MediaIcons/ThumbButtons/forward.ico"), Application.Current.FindResource("nexttrack").ToString());
            thumbnailButtonNext.Click += (s, e) => { ViewModels.MainViewModel.Instance.MusicEngine.GoForward(); };

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

        void CSCoreEngine_StartVisualization(object sender, EventArgs e)
        {
            SpectrumAnalyzer.RegisterSoundPlayer(ViewModels.MainViewModel.Instance.MusicEngine.CSCoreEngine);
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModels.MainViewModel.Instance.Closing();
        }

        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
            e.Effects = DragDropEffects.Move;
        }
    }
}
