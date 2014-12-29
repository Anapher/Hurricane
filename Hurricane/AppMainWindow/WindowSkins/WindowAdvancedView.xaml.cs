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
    /// Interaction logic for WindowAdvancedView.xaml
    /// </summary>
    public partial class WindowAdvancedView : UserControl, IWindowSkin
    {
        private Hurricane.Resources.Styles.DragDropListView.ServiceProviders.UI.ListViewDragDropManager<Music.Track> dragMgr;
        public WindowAdvancedView()
        {
            InitializeComponent();
            dragMgr = new Resources.Styles.DragDropListView.ServiceProviders.UI.ListViewDragDropManager<Music.Track>(this.listview);
            dragMgr.ShowDragAdorner = true;
            this.Configuration = new WindowSkinConfiguration() { MaxHeight = double.PositiveInfinity, MaxWidth = double.PositiveInfinity, MinHeight = 500, MinWidth = 850, ShowSystemMenuOnRightClick = true, ShowTitleBar = false, ShowWindowControls = true, NeedMovingHelp = true, ShowFullscreenDialogs = true };
            ViewModels.SettingsViewModel.Instance.Load();
        }

        public event EventHandler DragMoveStart;

        public event EventHandler DragMoveStop;

        public event EventHandler CloseRequest
        {
            add { }
            remove { }
        }

        public event EventHandler ToggleWindowState;

        public void EnableWindow()
        {
           spektrum1.RefreshInterval = 20;
           if (manager != null) manager.CSCoreEngine.TrackChanged += CSCoreEngine_TrackChanged;
        }

        public void DisableWindow()
        {
            spektrum1.RefreshInterval = int.MaxValue;
            manager.CSCoreEngine.TrackChanged -= CSCoreEngine_TrackChanged;
        }

        public void RegisterSoundPlayer(Music.CSCoreEngine engine)
        {
            spektrum1.RegisterSoundPlayer(engine);
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

        #region Animations
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Storyboard storyb = new Storyboard();
            DoubleAnimation da = new DoubleAnimation(0.3, 1, TimeSpan.FromMilliseconds(500));
            ThicknessAnimation ta = new ThicknessAnimation(new Thickness(-10, 0, 10, 0), new Thickness(0), TimeSpan.FromSeconds(0.4));
            Storyboard.SetTarget(da, gridplaylist);
            Storyboard.SetTarget(ta, gridplaylist);
            Storyboard.SetTargetProperty(da, new PropertyPath(FrameworkElement.OpacityProperty));
            Storyboard.SetTargetProperty(ta, new PropertyPath(FrameworkElement.MarginProperty));

            storyb.Children.Add(da);
            storyb.Children.Add(ta);
            storyb.Begin(this);
        }

        private Storyboard FadeInAnimation(int interval, params FrameworkElement[] controls)
        {
            Storyboard story = new Storyboard();
            int counter = 0;
            foreach (var control in controls)
            {
                control.BeginAnimation(FrameworkElement.OpacityProperty, null);
                control.BeginAnimation(FrameworkElement.MarginProperty, null);
                control.Opacity = 0;
                control.Margin = new Thickness(0, control.Margin.Top, 0, 0);
                DoubleAnimation da = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                ThicknessAnimation ta = new ThicknessAnimation(new Thickness(-10, control.Margin.Top, 10, 0), new Thickness(0, control.Margin.Top, 0, 0), TimeSpan.FromMilliseconds(400));
                Storyboard.SetTarget(da, control);
                Storyboard.SetTarget(ta, control);
                Storyboard.SetTargetProperty(da, new PropertyPath(FrameworkElement.OpacityProperty));
                Storyboard.SetTargetProperty(ta, new PropertyPath(FrameworkElement.MarginProperty));
                story.Children.Add(da);
                story.Children.Add(ta);
                da.BeginTime = TimeSpan.FromMilliseconds(counter * interval);
                ta.BeginTime = TimeSpan.FromMilliseconds(counter * interval);
                counter++;
            }

            story.Completed += (s, e) =>
            {
                foreach (var c in controls)
                {
                    c.Opacity = 1;
                } 
            };
            return story;
        }
        #endregion

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DragMoveStart != null) DragMoveStart(this, EventArgs.Empty);
            if (e.ClickCount == 2) if (ToggleWindowState != null) ToggleWindowState(this, EventArgs.Empty);
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DragMoveStop != null) DragMoveStop(this, EventArgs.Empty);
        }

        protected Storyboard story1;
        protected Storyboard story2;
        void CSCoreEngine_TrackChanged(object sender, Music.TrackChangedEventArgs e)
        {
            if (story1 != null) { story1.Stop(this); }
            if (story2 != null) { story2.Stop(this);  }
            story1 = FadeInAnimation(300, txt1, txt2, fav, stack1, stack2, stack3);
            story2 = FadeInAnimation(300, full1, full2, full3, fullfav);
            story1.Begin(this, true);
            story2.Begin(this, true);
        }

        private Music.MusicManager manager;
        public void MusicManagerEnabled(object manager)
        {
            this.manager = (Music.MusicManager)manager;
            this.manager.CSCoreEngine.TrackChanged += CSCoreEngine_TrackChanged;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listview = (ListView)sender;
            listview.ScrollIntoView(listview.SelectedItem);
        }

        private void SettingChanged(object sender, RoutedEventArgs e)
        {
            ViewModels.SettingsViewModel.Instance.StateChanged();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString().Length == 1 && char.IsLetterOrDigit(Convert.ToChar(e.Key.ToString())) && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftAlt))
            {
                txtSearch.Focus();
            }
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
                ViewModels.MainViewModel.Instance.DragDropFiles((string[])e.Data.GetData(DataFormats.FileDrop));
            }
        }
    }
}
