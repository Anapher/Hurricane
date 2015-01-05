using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Hurricane.Music;

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

        public static readonly DependencyProperty TrackProperty = DependencyProperty.Register("Track", typeof(Track), typeof(TrackImagePreseter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TrackChangedCallback));

        private static void TrackChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var imagepresenter = (TrackImagePreseter) dependencyObject;
            imagepresenter.TrackChanged((Track)dependencyPropertyChangedEventArgs.NewValue);
        }

        protected void TrackChanged(Track newTrack)
        {
            this.Track = newTrack;
            if(PropertyChanged != null)PropertyChanged(this, new PropertyChangedEventArgs("Track"));
        }

        public Track Track
        {
            get { return (Track)GetValue(TrackProperty); }
            set { SetValue(TrackProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
