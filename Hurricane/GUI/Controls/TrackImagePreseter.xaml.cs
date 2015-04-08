using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Hurricane.Music.Data;

namespace Hurricane.GUI.Controls
{
    /// <summary>
    /// Interaction logic for TrackImagePreseter.xaml
    /// </summary>
    public partial class TrackImagePreseter : INotifyPropertyChanged
    {
        public TrackImagePreseter()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TrackProperty = DependencyProperty.Register("Track", typeof(IRepresentable), typeof(TrackImagePreseter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TrackChangedCallback));

        private static void TrackChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var imagepresenter = (TrackImagePreseter)dependencyObject;
            imagepresenter.TrackChanged((IRepresentable)dependencyPropertyChangedEventArgs.NewValue);
        }

        protected void TrackChanged(IRepresentable newTrack)
        {
            SetTrack(newTrack);
        }

        public IRepresentable Track
        {
            get { return (IRepresentable)GetValue(TrackProperty); }
            set { SetValue(TrackProperty, value); }
        }

        public IRepresentable TrackToRepresent { get; private set; }

        private CancellationTokenSource _cts;
        private async void SetTrack(IRepresentable newTrack)
        {
            if (_cts != null)
                _cts.Cancel();

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            if (newTrack != null)
            {
                var counter = 0;
                while (newTrack.Image == null)
                {
                    counter++;
                    if (counter > 15) break;
                    try
                    {
                        await Task.Delay(100, token);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                }
            }

            if (!token.IsCancellationRequested)
            {
                TrackToRepresent = newTrack;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("TrackToRepresent"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
