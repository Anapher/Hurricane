using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Hurricane.ViewModel.Annotations;

namespace Hurricane.Controls
{
    /// <summary>
    /// Interaktionslogik für PieProgress.xaml
    /// </summary>
    public partial class PieProgress : INotifyPropertyChanged
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(double), typeof(PieProgress), new PropertyMetadata(default(double), ValuePropertyChangedCallback));

        public static readonly DependencyProperty PieBrushProperty = DependencyProperty.Register(
            "PieBrush", typeof(Brush), typeof(PieProgress), new PropertyMetadata(Brushes.BlueViolet));

        public static readonly DependencyProperty CanCancelProperty = DependencyProperty.Register(
            "CanCancel", typeof (bool), typeof (PieProgress), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof (ICommand), typeof (PieProgress), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter", typeof (object), typeof (PieProgress), new PropertyMetadata(default(object)));

        private bool _showCancel;

        public PieProgress()
        {
            InitializeComponent();
            SizeChanged += PieProgress_SizeChanged;
            Loaded += (sender, args) =>
            {
                Resize();
                DrawSector();
            };
            MouseEnter += PieProgress_MouseEnter;
            MouseLeave += PieProgress_MouseLeave;
            MouseUp += PieProgress_MouseUp;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public Brush PieBrush
        {
            get { return (Brush)GetValue(PieBrushProperty); }
            set { SetValue(PieBrushProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public bool CanCancel
        {
            get { return (bool)GetValue(CanCancelProperty); }
            set { SetValue(CanCancelProperty, value); }
        }

        public bool ShowCancel
        {
            get { return _showCancel; }
            set
            {
                if (_showCancel != value)
                {
                    _showCancel = value;
                    OnPropertyChanged();
                }
            }
        }

        private static void ValuePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var pieProgress = dependencyObject as PieProgress;
            if (pieProgress == null)
                throw new ArgumentException("dependencyObject");

            pieProgress.DrawSector();
        }

        private void PieProgress_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Resize();
        }

        private void PieProgress_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CanCancel)
                Command?.Execute(CommandParameter);
        }

        private void PieProgress_MouseLeave(object sender, MouseEventArgs e)
        {
            ShowCancel = false;
        }

        private void PieProgress_MouseEnter(object sender, MouseEventArgs e)
        {
            if (CanCancel)
            {
                ShowCancel = true;
            }
        }

        public void DrawSector()
        {
            Path.Data = null;

            if (Value.Equals(0))
                return;

            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();

            var radius = ActualHeight / 2;
            var theta = (360 * Value) - 90;  // <--- the coordinate system is flipped with 0,0 at top left. Hence the -90

            if (Value.Equals(1))
                theta += 1;

            var finalPointX = radius + (radius * Math.Cos(theta * 0.0174));
            var finalPointY = radius + (radius * Math.Sin(theta * 0.0174));

            pathFigure.StartPoint = new Point(radius, radius);
            var firstLine = new LineSegment(new Point(radius, 0), true);
            pathFigure.Segments.Add(firstLine);

            if (Value > 0.25)
            {
                var firstQuart = new ArcSegment
                {
                    Point = new Point(ActualWidth, radius),
                    SweepDirection = SweepDirection.Clockwise,
                    IsStroked = true,
                    Size = new Size(radius, radius)
                };
                pathFigure.Segments.Add(firstQuart);
            }

            if (Value > 0.5)
            {
                var secondQuart = new ArcSegment
                {
                    Point = new Point(radius, ActualHeight),
                    SweepDirection = SweepDirection.Clockwise,
                    IsStroked = true,
                    Size = new Size(radius, radius)
                };
                pathFigure.Segments.Add(secondQuart);
            }

            if (Value > 0.75)
            {
                ArcSegment thirdQuart = new ArcSegment
                {
                    Point = new Point(0, radius),
                    SweepDirection = SweepDirection.Clockwise,
                    IsStroked = true,
                    Size = new Size(radius, radius)
                };
                pathFigure.Segments.Add(thirdQuart);
            }

            var finalQuart = new ArcSegment
            {
                Point = new Point(finalPointX, finalPointY),
                SweepDirection = SweepDirection.Clockwise,
                IsStroked = true,
                Size = new Size(radius, radius)
            };
            pathFigure.Segments.Add(finalQuart);

            var lastLine = new LineSegment(new Point(radius, radius), true);
            pathFigure.Segments.Add(lastLine);

            pathGeometry.Figures.Add(pathFigure);
            Path.Data = pathGeometry;
        }

        private void Resize()
        {
            Height = ActualWidth;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
