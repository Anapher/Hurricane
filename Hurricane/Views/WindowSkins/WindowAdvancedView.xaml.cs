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

namespace Hurricane.Views.WindowSkins
{
    /// <summary>
    /// Interaktionslogik für WindowAdvancedView.xaml
    /// </summary>
    public partial class WindowAdvancedView : UserControl, IWindowSkin
    {
        public WindowAdvancedView()
        {
            InitializeComponent();
            this.Configuration = new WindowSkinConfiguration() { MaxHeight = double.PositiveInfinity, MaxWidth = double.PositiveInfinity, MinHeight = 500, MinWidth = 850, ShowSystemMenuOnRightClick = true, ShowTitleBar = false, ShowWindowControls = true, NeedMovingHelp = true };
            ViewModels.SettingsViewModel.Instance.Load();
        }

        public event EventHandler DragMoveStart;

        public event EventHandler DragMoveStop;

        public event EventHandler CloseRequest;

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

        private void FadeInAnimation(TimeSpan BeginTime, FrameworkElement control)
        {
            control.BeginAnimation(FrameworkElement.OpacityProperty, null);
            control.Opacity = 0;
            Storyboard story = new Storyboard();
            DoubleAnimation da = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            ThicknessAnimation ta = new ThicknessAnimation(new Thickness(-10, 0, 10, 0), new Thickness(0), TimeSpan.FromMilliseconds(400));
            Storyboard.SetTarget(da, control);
            Storyboard.SetTarget(ta, control);
            Storyboard.SetTargetProperty(da, new PropertyPath(FrameworkElement.OpacityProperty));
            Storyboard.SetTargetProperty(ta, new PropertyPath(FrameworkElement.MarginProperty));

            story.Children.Add(da);
            story.Children.Add(ta);
            da.BeginTime = BeginTime;
            ta.BeginTime = BeginTime;
            story.Completed += (s, e) => { control.Opacity = 1; };
            story.Begin(this);
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

        void CSCoreEngine_TrackChanged(object sender, Music.TrackChangedEventArgs e)
        {
            FadeInAnimation(TimeSpan.Zero, txt1);
            FadeInAnimation(TimeSpan.FromMilliseconds(300), txt2);

            FadeInAnimation(TimeSpan.Zero, full1);
            FadeInAnimation(TimeSpan.FromMilliseconds(300), full2);
            FadeInAnimation(TimeSpan.FromMilliseconds(600), full3);
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
    }
}
