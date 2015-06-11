using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Hurricane.Controls
{
    [ContentProperty("Content"),
        TemplatePart(Name = "PART_MainContent", Type = typeof(ContentPresenter)),
        TemplatePart(Name = "PART_PaintArea", Type = typeof(Shape))]
    class AnimationUpDown : Control
    {
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(List<FrameworkElement>), typeof(AnimationUpDown), new PropertyMetadata(default(List<FrameworkElement>)));

        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex", typeof(int), typeof(AnimationUpDown), new PropertyMetadata(-1, PropertyChangedCallback));

        public static readonly DependencyProperty CurrentContentProperty = DependencyProperty.Register(
            "CurrentContent", typeof(FrameworkElement), typeof(AnimationUpDown), new PropertyMetadata(default(FrameworkElement)));

        public static readonly DependencyProperty DistanceTopProperty = DependencyProperty.Register(
            "DistanceTop", typeof (double), typeof (AnimationUpDown), new PropertyMetadata(default(double)));

        public static readonly DependencyProperty DistanceBottomProperty = DependencyProperty.Register(
            "DistanceBottom", typeof (double), typeof (AnimationUpDown), new PropertyMetadata(default(double)));

        private Shape _paintAera;
        private ContentPresenter _mainContent;

        public AnimationUpDown()
        {
            Content = new List<FrameworkElement>();
            Loaded += AnimationUpDown_Loaded;
        }

        private void AnimationUpDown_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            if (SelectedIndex > -1)
                CurrentContent = Content[SelectedIndex];
        }

        public List<FrameworkElement> Content
        {
            get { return (List<FrameworkElement>)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public FrameworkElement CurrentContent
        {
            get { return (FrameworkElement)GetValue(CurrentContentProperty); }
            set { SetValue(CurrentContentProperty, value); }
        }

        public double DistanceBottom
        {
            get { return (double)GetValue(DistanceBottomProperty); }
            set { SetValue(DistanceBottomProperty, value); }
        }

        public double DistanceTop
        {
            get { return (double)GetValue(DistanceTopProperty); }
            set { SetValue(DistanceTopProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            _paintAera = Template.FindName("PART_PaintArea", this) as Shape;
            _mainContent = Template.FindName("PART_MainContent", this) as ContentPresenter;
            base.OnApplyTemplate();
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyPropertyChangedEventArgs.NewValue == dependencyPropertyChangedEventArgs.OldValue)
                return;

            var animationControl = dependencyObject as AnimationUpDown;
            if (animationControl == null)
                throw new InvalidOperationException();

            if (!animationControl.IsLoaded)
                return;

            if ((int) dependencyPropertyChangedEventArgs.NewValue < -1 ||
                animationControl.Content == null ||
                (int) dependencyPropertyChangedEventArgs.NewValue > animationControl.Content.Count - 1)
                throw new ArgumentException();

            if ((int) dependencyPropertyChangedEventArgs.NewValue == -1)
                return;

            animationControl.BeginAnimation((int)dependencyPropertyChangedEventArgs.NewValue);
        }

        private void BeginAnimation(int newIndex)
        {
            _paintAera.Fill = CreateBrushFromVisual(_mainContent, (int)ActualWidth, (int)ActualHeight);
            var newContent = Content[newIndex];
            var oldIndex = Content.IndexOf(CurrentContent);

            var newContentTransform = new TranslateTransform();
            var oldContentTransform = new TranslateTransform();

            _paintAera.RenderTransform = oldContentTransform;
            _mainContent.RenderTransform = newContentTransform;
            _paintAera.Visibility = Visibility.Visible;
            CurrentContent = newContent;

            var moveDown = newIndex > oldIndex;
            newContentTransform.BeginAnimation(TranslateTransform.YProperty,
                CreateAnimation(moveDown ? -ActualHeight - DistanceBottom : ActualHeight + DistanceTop, 0));
            oldContentTransform.BeginAnimation(TranslateTransform.YProperty,
                CreateAnimation(0, moveDown ? ActualHeight + DistanceTop : -ActualHeight - DistanceBottom,
                    (s, e) => _paintAera.Visibility = Visibility.Hidden));
        }

        private AnimationTimeline CreateAnimation(double from, double to, EventHandler whenDone = null)
        {
            var duration = new Duration(TimeSpan.FromSeconds(0.5));
            var anim = new DoubleAnimation(from, to, duration);
            if (whenDone != null)
                anim.Completed += whenDone;

            anim.Freeze();
            return anim;
        }

        private static Brush CreateBrushFromVisual(Visual v, int width, int height)
        {
            if (v == null)
                throw new ArgumentNullException(nameof(v));
            var target = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            target.Render(v);
            var brush = new ImageBrush(target);
            brush.Freeze();
            return brush;
        }
    }
}
