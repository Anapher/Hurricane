using System.ComponentModel;
using System.Windows;
using Hurricane.Music.Data;

namespace Hurricane.Extensions
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
            var imagepresenter = (TrackImagePreseter) dependencyObject;
            imagepresenter.TrackChanged((IRepresentable)dependencyPropertyChangedEventArgs.NewValue);
        }

        protected void TrackChanged(IRepresentable newTrack)
        {
            if(PropertyChanged != null)PropertyChanged(this, new PropertyChangedEventArgs("Track"));
        }

        public IRepresentable Track
        {
            get { return (IRepresentable)GetValue(TrackProperty); }
            set { SetValue(TrackProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
