using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Hurricane.MagicArrow;
using Hurricane.Model.Skin;
using Hurricane.Utilities;
using MahApps.Metro.Controls;

namespace Hurricane.Controls
{
    public partial class MagicMetroWindow : MetroWindow
    {
        public static readonly DependencyProperty NormalWindowSkinProperty = DependencyProperty.Register(
            "NormalWindowSkin", typeof (IWindowSkin), typeof (MagicMetroWindow), new PropertyMetadata(default(IWindowSkin)));

        public static readonly DependencyProperty DockWindowSkinProperty = DependencyProperty.Register(
            "DockWindowSkin", typeof(IWindowSkin), typeof(MagicMetroWindow), new PropertyMetadata(default(IWindowSkin)));

        public static readonly DependencyProperty CurrentSideProperty = DependencyProperty.Register(
            "CurrentSide", typeof (DockingSide), typeof (MagicMetroWindow), new PropertyMetadata(default(DockingSide), CurrentSidePropertyChangedCallback));

        public static readonly DependencyProperty ShowMagicArrowBelowCursorProperty = DependencyProperty.Register(
            "ShowMagicArrowBelowCursor", typeof(bool), typeof(MagicMetroWindow), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(
            "CloseCommand", typeof (ICommand), typeof (MagicMetroWindow), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty MinimizeToTrayProperty = DependencyProperty.Register(
            "MinimizeToTray", typeof (bool), typeof (MagicMetroWindow), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty MinimizeToTrayMessageProperty = DependencyProperty.Register(
            "MinimizeToTrayMessage", typeof (string), typeof (MagicMetroWindow), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ShowMessageWhenMinimizeToTrayProperty = DependencyProperty.Register(
            "ShowMessageWhenMinimizeToTray", typeof (bool), typeof (MagicMetroWindow), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty WindowWidthProperty = DependencyProperty.Register(
            "WindowWidth", typeof (double), typeof (MagicMetroWindow), new PropertyMetadata(default(double),
                (o, args) =>
                {
                    var window = o as Window;
                    if (window == null) throw new ArgumentException(o.ToString());
                    var newWidth = (double) args.NewValue;
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (window.Width != newWidth) window.Width = newWidth;
                }));

        public static readonly DependencyProperty WindowHeightProperty = DependencyProperty.Register(
            "WindowHeight", typeof (double), typeof (MagicMetroWindow), new PropertyMetadata(default(double),
                (o, args) =>
                {
                    var window = o as Window;
                    if (window == null) throw new ArgumentException(o.ToString());
                    var newHeight = (double) args.NewValue;
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (window.Height != newHeight) window.Height = newHeight;
                }));

        public static readonly DependencyProperty WindowLeftProperty = DependencyProperty.Register(
            "WindowLeft", typeof (double), typeof (MagicMetroWindow), new PropertyMetadata(default(double),
                (o, args) =>
                {
                    var window = o as Window;
                    if (window == null) throw new ArgumentException(o.ToString());
                    var newLeft = (double) args.NewValue;
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (window.Left != newLeft) window.Left = newLeft;
                }));

        public static readonly DependencyProperty WindowTopProperty = DependencyProperty.Register(
            "WindowTop", typeof (double), typeof (MagicMetroWindow), new PropertyMetadata(default(double),
                (o, args) =>
                {
                    var window = o as Window;
                    if (window == null) throw new ArgumentException(o.ToString());
                    var newTop = (double)args.NewValue;
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (window.Top != newTop) window.Top = newTop;
                }));

        private MagicArrowService _magicArrow;
        private IWindowSkin _defaultNormalWindowSkin;
        private IWindowSkin _defaultDockedWindowSkin;

        public MagicMetroWindow()
        {
            SourceInitialized += MagicMetroWindow_SourceInitialized;
            Closing += MagicMetroWindow_Closing;
            StateChanged += MagicMetroWindow_StateChanged;
            _defaultNormalWindowSkin = new WindowSkinNormal.WindowSkinNormal();
            _defaultDockedWindowSkin = new WindowSkinDocked.WindowSkinDocked();
        }

        void MagicMetroWindow_Closing(object sender, CancelEventArgs e)
        {
            if (CurrentWindowState == CurrentWindowState.Normal) //We save the position
            {
                WindowHeight = Height;
                WindowWidth = Width;
                WindowLeft = Left;
                WindowTop = Top;
            }

            CloseCommand?.Execute(e);
            _magicArrow.Dispose();
        }

        void MagicMetroWindow_SourceInitialized(object sender, EventArgs e)
        {
            _magicArrow = new MagicArrowService(this);
            _magicArrow.DockManager.Docked += (s, args) =>
            {
                CurrentWindowState = CurrentWindowState.Docked;
                ApplyWindowSkin();
                CurrentSide = args.DockingSide;
            };
            _magicArrow.DockManager.Undocked += (o, args) =>
            {
                CurrentWindowState = CurrentWindowState.Normal;
                ApplyWindowSkin();
                CurrentSide = DockingSide.None;
            };
            _magicArrow.DockManager.DragStopped += DockManager_DragStopped;
            _magicArrow.DockManager.CurrentSide = CurrentSide;

            ApplyWindowSkin();
        }

        public IWindowSkin NormalWindowSkin
        {
            get { return (IWindowSkin) GetValue(NormalWindowSkinProperty); }
            set { SetValue(NormalWindowSkinProperty, value); }
        }

        public IWindowSkin DockWindowSkin
        {
            get { return (IWindowSkin) GetValue(DockWindowSkinProperty); }
            set { SetValue(DockWindowSkinProperty, value); }
        }

        public DockingSide CurrentSide
        {
            get { return (DockingSide)GetValue(CurrentSideProperty); }
            set { SetValue(CurrentSideProperty, value); }
        }

        public bool ShowMagicArrowBelowCursor
        {
            get { return (bool)GetValue(ShowMagicArrowBelowCursorProperty); }
            set { SetValue(ShowMagicArrowBelowCursorProperty, value); }
        }

        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }

        public bool MinimizeToTray
        {
            get { return (bool)GetValue(MinimizeToTrayProperty); }
            set { SetValue(MinimizeToTrayProperty, value); }
        }

        public string MinimizeToTrayMessage
        {
            get { return (string)GetValue(MinimizeToTrayMessageProperty); }
            set { SetValue(MinimizeToTrayMessageProperty, value); }
        }

        public bool ShowMessageWhenMinimizeToTray
        {
            get { return (bool)GetValue(ShowMessageWhenMinimizeToTrayProperty); }
            set { SetValue(ShowMessageWhenMinimizeToTrayProperty, value); }
        }

        public double WindowWidth
        {
            get { return (double)GetValue(WindowWidthProperty); }
            set { SetValue(WindowWidthProperty, value); }
        }

        public double WindowHeight
        {
            get { return (double)GetValue(WindowHeightProperty); }
            set { SetValue(WindowHeightProperty, value); }
        }

        public double WindowLeft
        {
            get { return (double)GetValue(WindowLeftProperty); }
            set { SetValue(WindowLeftProperty, value); }
        }

        public double WindowTop
        {
            get { return (double)GetValue(WindowTopProperty); }
            set { SetValue(WindowTopProperty, value); }
        }

        public IWindowSkin CurrentView { get; set; }
        public CurrentWindowState CurrentWindowState { get; set; }

        public void RefreshView()
        {
            WindowHeight = Height;
            WindowWidth = Width;
            WindowLeft = Left;
            WindowTop = Top;

            _defaultNormalWindowSkin = new WindowSkinNormal.WindowSkinNormal();
            _defaultDockedWindowSkin = new WindowSkinDocked.WindowSkinDocked();
            ApplyWindowSkin();
        }

        protected void ApplyWindowSkin()
        {
            var newWindowSkin = CurrentWindowState == CurrentWindowState.Normal ? (NormalWindowSkin ?? _defaultNormalWindowSkin) : (DockWindowSkin ?? _defaultDockedWindowSkin);
            if (CurrentView == newWindowSkin)
                return;

            if (CurrentView != null)
            {
                CurrentView.DragMoveStart -= WindowSkin_DragMoveStart;
                CurrentView.DragMoveStop -= WindowSkin_DragMoveStop;
                CurrentView.ToggleWindowState -= WindowSkin_ToggleWindowState;
                CurrentView.TitleBarMouseMove -= WindowSkin_TitleBarMouseMove;
            }

            //Handle events
            newWindowSkin.CloseRequest += WindowSkin_CloseRequest;
            newWindowSkin.DragMoveStart += WindowSkin_DragMoveStart;
            newWindowSkin.DragMoveStop += WindowSkin_DragMoveStop;
            newWindowSkin.ToggleWindowState += WindowSkin_ToggleWindowState;
            newWindowSkin.TitleBarMouseMove += WindowSkin_TitleBarMouseMove;

            //if (!_isDragging)
                ResizeMode = newWindowSkin.Configuration.IsResizable ? ResizeMode.CanResize : ResizeMode.NoResize;

            if (CurrentWindowState == CurrentWindowState.Normal)
            {
                Height = WindowHeight;
                Width = WindowWidth;
            }
            else
            {
                WindowHeight = Height;
                WindowWidth = Width;
                WindowTop = Top;
                WindowLeft = Left;
            }

            //Set properties
            MaxHeight = newWindowSkin.Configuration.MaxHeight;
            MinHeight = newWindowSkin.Configuration.MinHeight;
            MaxWidth = newWindowSkin.Configuration.MaxWidth;
            MinWidth = newWindowSkin.Configuration.MinWidth;
            ShowTitleBar = newWindowSkin.Configuration.ShowTitleBar;
            ShowSystemMenuOnRightClick = newWindowSkin.Configuration.ShowSystemMenuOnRightClick;

            if (CurrentWindowState == CurrentWindowState.Docked)
            {
                Width = 300;
            }

            Content = newWindowSkin;
            CurrentView = newWindowSkin;
        }

        private static void CurrentSidePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var magicMetroWindow = dependencyObject as MagicMetroWindow;
            if (magicMetroWindow == null)
                throw new ArgumentException(nameof(dependencyObject));
            if (dependencyPropertyChangedEventArgs.NewValue == dependencyPropertyChangedEventArgs.OldValue) return;
            magicMetroWindow.ApplySide((DockingSide) dependencyPropertyChangedEventArgs.NewValue);
        }

        private void ApplySide(DockingSide side)
        {
            switch (side)
            {
                case DockingSide.None:

                    break;
                case DockingSide.Left:
                case DockingSide.Right:
                    Left = side == DockingSide.Left ? WpfScreen.MostLeftX : WpfScreen.MostRightX - 300;
                    var screen = WpfScreen.GetScreenFrom(new Point(Left, 0));
                    Top = screen.WorkingArea.Top;
                    Height = screen.WorkingArea.Height;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void WindowSkin_ToggleWindowState(object sender, EventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void WindowSkin_CloseRequest(object sender, EventArgs e)
        {
            Close();
        }

        void MagicMetroWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && MinimizeToTray)
                Hide();
        }
    }

    public enum CurrentWindowState
    {
        Normal,
        Docked
    }
}