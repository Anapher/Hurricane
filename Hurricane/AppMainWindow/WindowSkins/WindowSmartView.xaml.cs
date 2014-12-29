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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hurricane.AppMainWindow.WindowSkins
{
    /// <summary>
    /// Interaction logic for WindowSmartView.xaml
    /// </summary>
    public partial class WindowSmartView : UserControl, IWindowSkin
    {
        private Hurricane.Resources.Styles.DragDropListView.ServiceProviders.UI.ListViewDragDropManager<Music.Track> dragMgr;
        public WindowSmartView()
        {
            InitializeComponent();
            dragMgr = new Resources.Styles.DragDropListView.ServiceProviders.UI.ListViewDragDropManager<Music.Track>(this.listview);
            dragMgr.ShowDragAdorner = true;
            this.Configuration = new WindowSkinConfiguration() {  MaxHeight = Utilities.WpfScreen.MaxHeight, MaxWidth = 300, MinHeight = 400, MinWidth  = 300, ShowSystemMenuOnRightClick = false, ShowTitleBar = false, ShowWindowControls = false, NeedMovingHelp = true, ShowFullscreenDialogs = false };
        }

        private void buttonplus_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = ((Button)sender).ContextMenu;
            menu.PlacementTarget = (UIElement)sender;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            menu.IsOpen = true;
        }

        #region CurrentTrackAnimation
        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid == null) return;
            CurrentTrackAnimation(grid, txtCurrentTrack, polyplay, true);
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid == null) return;
            CurrentTrackAnimation(grid, txtCurrentTrack, polyplay, false);
        }

        private void CurrentTrackAnimation(Grid grid, TextBlock txt, Polygon poly, bool InAnimate)
        {
            Storyboard story = new Storyboard();

            ColorAnimation coloranimation2 = new ColorAnimation(InAnimate ? ((SolidColorBrush)Application.Current.FindResource("DarkColorBrush")).Color : Colors.Black, TimeSpan.FromMilliseconds(250));
            Storyboard.SetTarget(coloranimation2, txtCurrentTrack);
            Storyboard.SetTargetProperty(coloranimation2, new PropertyPath("Foreground.Color"));

            ThicknessAnimation thicknessanimation = new ThicknessAnimation(InAnimate ? new Thickness(3, 2, -3, 0) : new Thickness(0, 2, 0, 0), TimeSpan.FromMilliseconds(250));
            Storyboard.SetTarget(thicknessanimation, poly);
            Storyboard.SetTargetProperty(thicknessanimation, new PropertyPath(Polygon.MarginProperty));

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
                ViewModels.MainViewModel.Instance.DragDropFiles((string[])e.Data.GetData(DataFormats.FileDrop));
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

        public void EnableWindow()
        {
            SpectrumAnalyzer.RefreshInterval = 20;
        }

        public void DisableWindow()
        {
            SpectrumAnalyzer.RefreshInterval = int.MaxValue;
        }

        public void RegisterSoundPlayer(Music.CSCoreEngine engine)
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

        private Music.MusicManager manager;
        public void MusicManagerEnabled(object manager)
        {
            this.manager = (Music.MusicManager)manager;
        }
    }
}
